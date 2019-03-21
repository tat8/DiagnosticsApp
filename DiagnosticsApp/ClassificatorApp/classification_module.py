import dicom
import os
import numpy as np
from matplotlib import pyplot as plt
from matplotlib import cm
from PIL import Image
import queue
from shutil import copyfile
from sklearn.ensemble import GradientBoostingClassifier
from sklearn.cluster import KMeans
from skimage import morphology
from skimage import measure
from read_dicom_files import read_dicoms_to_images
import natsort
from timeit import default_timer as timer


def get_dicom_array(dcm_path):
    PathDicom = dcm_path
    lstFilesDCM = []  # create an empty list
    for dirName, subdirList, fileList in os.walk(PathDicom):
        for filename in fileList:
            if ".dcm" in filename.lower():  # check whether the file's DICOM
                lstFilesDCM.append(os.path.join(dirName,filename))
    # sort files
    lstFilesDCM = natsort.natsorted(lstFilesDCM)

    # Get ref file
    RefDs = dicom.read_file(lstFilesDCM[0])

    # Load dimensions based on the number of rows, columns, and slices (along the Z axis)
    ConstPixelDims = (len(lstFilesDCM), int(RefDs.Rows), int(RefDs.Columns))
    # The array is sized based on 'ConstPixelDims'
    ArrayDicom = np.zeros(ConstPixelDims, dtype=RefDs.pixel_array.dtype)

    for filenameDCM in lstFilesDCM:
        # read the file
        ds = dicom.read_file(filenameDCM)
        # store the raw image data
        # добавить словарь для lstFilesDCM ?
        ArrayDicom[lstFilesDCM.index(filenameDCM)] = ds.pixel_array  

    return ArrayDicom


def get_lungs_masks_for_slices(dicom_slices_array):
    """
    Performs the slices segmentation and returns a lungs mask for all slices
    :param dicom_slices_array: ndarray of original dicom slices pixel data
    :return: ndarray of dicom slices pixel data where values are equal to 1 or 0:
     1 is the pixel belonging to lungs, 0 is the pixel belonging to other structures
    """
    segmented_slices = []
    for s in dicom_slices_array:
        segmented_slices.append(get_lungs_mask_for_slice(s))

    return np.stack(segmented_slices)


def get_lungs_mask_for_slice(slice_array):
    """
    Performs the slices segmentation and returns a lungs mask
    :param slice_array: ndarray of a dicom slice data
    :return: ndarray of dicom slice data where values are equal to 1 or 0:
     1 is the pixel belonging to lungs, 0 is the pixel belonging to other structures
    """
    ls_conf = {
            'erosion_filter_size': 3,
            'dilation_filter_size': 9
    }

    rows_num = slice_array.shape[0]
    cols_num = slice_array.shape[1]

    # Global image normalization
    global_mean = np.mean(slice_array)
    global_std = np.std(slice_array)
    norm_slice = slice_array - global_mean
    norm_slice = norm_slice / global_std

    # Find the mean value for the middle area of the slice
    middle_slice = norm_slice[int(cols_num / 5): int(cols_num / 5 * 4),
                              int(rows_num / 5): int(rows_num / 5 * 4)]
    middle_mean = np.mean(middle_slice)

    # Smoothing the intensity peaks by middle mean value
    norm_slice_min = np.min(norm_slice)
    norm_slice_max = np.max(norm_slice)
    norm_slice[norm_slice == norm_slice_min] = middle_mean
    norm_slice[norm_slice == norm_slice_max] = middle_mean

    # Use k-means to separate tissue background and air foreground of the middle
    kmeans = KMeans(n_clusters=2).\
        fit(np.reshape(middle_slice, [np.prod(middle_slice.shape), 1]))
    cluster_centers = sorted(kmeans.cluster_centers_.flatten())

    # Thresholding of the slice
    intencity_threshold = np.mean(cluster_centers)
    thresholded_slice = np.where(norm_slice < intencity_threshold, 1.0, 0.0)

    # Make erosion and dilation to smooth noises
    ef_size = ls_conf['erosion_filter_size']
    df_size = ls_conf['dilation_filter_size']
    eroded_slice = morphology.erosion(thresholded_slice, np.ones([ef_size, ef_size]))
    dilated_slice = morphology.dilation(eroded_slice, np.ones([df_size, df_size]))

    # Find regions which are fit to the estimated position of lungs
    labels = measure.label(dilated_slice)
    regions = measure.regionprops(labels)
    fit_labels = []
    for prop in regions:
        bbox = prop.bbox
        if bbox[2] - bbox[0] < rows_num / 10 * 9 and \
           bbox[3] - bbox[1] < cols_num / 10 * 9 and \
           bbox[0] > rows_num / 9 and \
           bbox[2] < cols_num / 9 * 8:
            fit_labels.append(prop.label)

    # Creating lungs mask
    lungs_mask = np.ndarray([rows_num, cols_num], dtype=np.uint8)
    lungs_mask[:] = 0
    for lb in fit_labels:
        lungs_mask = lungs_mask + np.where(labels == lb, 1, 0)
    lungs_mask = morphology.dilation(lungs_mask, np.ones([df_size, df_size]))
       
    return lungs_mask


def get_border_masks(imgs_path, polygon_color = (237,28,36)):
    # for each slice returns array of pixels colored in polygon_color - borders of the demanded region

    # get all img names
    file_names = sorted(os.listdir(imgs_path))
    image_names = []
    for fn in file_names:
        file_extension = fn.split('.')[-1]
        if file_extension == 'bmp':
            image_names.append(fn)

    # create label masks for images
    label_masks = []
    for img_name in image_names:
        img_path = os.path.join(imgs_path, img_name)
        img = Image.open(img_path).convert('RGB')
        img_array = np.array(img)
        label_masks.append(get_border_mask(img_array, img_path, polygon_color))
        print(img_name)

    return label_masks


def remove_pseudo_lungs_mask_for_slice(lungs_mask, dicom_array, stop_color):
    # bfs from zeros (that are neighbours with 1-pixels) in lungs_mask until meet color less than stop_color in dicom_array
    
    height, width = lungs_mask.shape
    visited = np.zeros(lungs_mask.shape)
    dx = [1,-1,0,0]
    dy = [0,0,1,-1]

    # fill queue_pixels with 0-pixels in lungs_mask that are neighbours with 1-pixels
    queue_pixels = queue.Queue()
    time_z = timer()
    for y in range(height):
        for x in range(width):
            if(lungs_mask[y][x]==0):
                for i in range(4):
                    y_neighbour = y + dy[i]
                    x_neighbour  = x + dx[i]
                    if((y_neighbour>0) and (y_neighbour<height) and (x_neighbour>0) and (x_neighbour<width) and (lungs_mask[y_neighbour][x_neighbour]==1)):
                            queue_pixels.put((y,x))
                            visited[y][x] = 1
                            break
    time_z2 = timer()
    # bfs
    while(queue_pixels.empty() == False):
        (yfrom, xfrom) = queue_pixels.get()
        for i in range(4):
            yto = yfrom + dy[i]
            xto = xfrom + dx[i]
            if((yto>0) and (yto<height) and (xto>0) and (xto<width)):
                if((lungs_mask[yto][xto] == 1) and (visited[yto][xto] == 0) and (dicom_array[yto][xto] > stop_color)):
                    visited[yto][xto] = 1
                    queue_pixels.put((yto,xto))
                    lungs_mask[yto][xto] = 0
    # erosion
    ef_size = 3
    lungs_mask = morphology.erosion(lungs_mask, np.ones([ef_size, ef_size]))
        
    print("Queue filling: {:g} secs, bfs running: {:g} secs".format(time_z2-time_z, timer() - time_z2))

    return lungs_mask


def remove_pseudo_lungs_mask(lungs_mask, dicom_array, stop_color=800):
    
    slice, _, _ = lungs_mask.shape
    
    for z in range(slice):
        lungs_mask[z] = remove_pseudo_lungs_mask_for_slice(lungs_mask[z], dicom_array[z], stop_color)

    return lungs_mask


def color_label_mask_for_slices(label_mask, bmp_image_path_from, bmp_image_path_to):
    images = os.listdir(bmp_image_path_from)
    images_number = len(images)
    for i in range(images_number):
        color_label_mask_for_slice(label_mask[i], bmp_image_path_from + '/' + images[i],
                                        bmp_image_path_to + '/colored_' + images[i])

    return


def color_label_mask_for_slice(label_mask, bmp_image_path_from, bmp_image_path_to):

    im_origin = Image.open(bmp_image_path_from)
    im = Image.new('RGB', im_origin.size)
    pix = im.load()
    height, width = label_mask.shape
    # color image
    for y in range(height):
            for x in range(width):
                if(label_mask[y][x]==1):
                    pix[x,y] = (0,200,0)
    # add transparence to an image
    im.putalpha(150)
    # combine original image and transparent image
    im_origin.paste(im.convert('RGB'), (0,0), im)
    im_origin.save(bmp_image_path_to)
    im_origin.close()
    im.close()

    return


#not in use
def remove_borders_from_lungs_mask(lungs_mask, dicom_array): #bfs от нулей?
    # removes pixels on the border of the lungs so only region inside lungs remains
    # uses the next method: 1) check the line from left to right.
    #                       when we meet value "1" at first time we understand that it is the border 
    #                       and mark it with "1" in lungs_mask_border,
    #                       do it until we meet black color (<800) in dicom_array. 
    #                       2) do the same from right to left  
    #                       3) check the line from up to down.
    #                       when we meet value "1" at first time we understand that it is the border 
    #                       and increment value in lungs_mask_border, 
    #                       so if lungs_mask_border[z][y][x] was "1", then now lungs_mask_border[z][y][x] = 2
    #                       do it until we meet black color (stop_color) in dicom_array. 
    #                       4) do the same as p.3 from down to up 
    #                       5) each pixel where lungs_mask_border[z][y][x] >= 2 is lungs border,
    #                       so we change lungs_mask[z][y][x] = 0 
    #                       (value can be more than 2 as there can be situation that we incremented the same at the p.3 and p.4)

    slice, height, width = lungs_mask.shape
    lungs_mask_border = np.zeros(lungs_mask.shape)
    stop_color = 800
    # indicates if the border has begun
    isBorder = False

    # from left to right
    for z in range(slice):
        for y in range(height):
            # indicates if the border has begun
            isBorder = False
            for x in range(width):
                if(lungs_mask[z][y][x] == 1):
                    isBorder = True
                if(isBorder):
                    if(dicom_array[z][y][x] < stop_color):
                        break
                    lungs_mask_border[z][y][x] = 1


    # from right to left
    for z in range(slice):
        for y in range(height):
            # indicates if the border has begun
            isBorder = False
            for x in range(width-1, -1, -1):
                if(lungs_mask[z][y][x] == 1):
                    isBorder = True
                if(isBorder):
                    if(dicom_array[z][y][x] < stop_color):
                        break
                    lungs_mask_border[z][y][x] = 1

    # from up to down
    for z in range(slice):
        for x in range(width):            
            # indicates if the border has begun
            isBorder = False
            for y in range(height):
                if(lungs_mask[z][y][x] == 1):
                    isBorder = True
                if(isBorder):
                    if(dicom_array[z][y][x] < stop_color):
                        break
                    lungs_mask_border[z][y][x] = 1

    # from down to up
    for z in range(slice):
        for x in range(width):            
            # indicates if the border has begun
            isBorder = False
            for y in range(height-1, -1, -1):
                if(lungs_mask[z][y][x] == 1):
                    isBorder = True
                if(isBorder):
                    if(dicom_array[z][y][x] < stop_color):
                        break
                    lungs_mask_border[z][y][x] = 1
    
    # remove border from lungs_mask using lungs_mask_border
    for z in range(slice):
        for y in range(height):
            for x in range(width):
                if(lungs_mask_border[z][y][x] == 1):
                    lungs_mask[z][y][x] = 0


    #im_origin = Image.open("test_border.bmp")
    #im = Image.new('RGB', im_origin.size)
    #pix = im.load()
    ## color image
    #for y in range(height):
    #        for x in range(width):
    #            if(lungs_mask_border[0][y][x]==1):
    #                pix[x,y] = (0,200,0)
    #            if(lungs_mask_border[0][y][x]==4):
    #                pix[x,y] = (200,200,200)
    #            if(lungs_mask_border[0][y][x]==2):
    #                pix[x,y] = (200,0,0)
    #            if(lungs_mask_border[0][y][x]==3):
    #                pix[x,y] = (0,0,200)
    ## add transparence to an image
    #im.save("test_border_mask.bmp")
    #im.putalpha(150)
    ## combine original image and transparent image
    #im_origin.paste(im.convert('RGB'), (0,0), im)
    #im_origin.save("test_border_res.bmp")
    #im_origin.close()
    #im.close()
    
    return lungs_mask


def get_border_mask(img_array, img_path, polygon_color = (237,28,36)):

        polygon_color_red = polygon_color[0]
        polygon_color_green = polygon_color[1]
        polygon_color_blue = polygon_color[2]
        height, width, _ = img_array.shape
        label_mask = []

        # Fill label mask by a certain number
        for y in range(height):
            for x in range(width):
                # ! comparison "if np.array_equal(img_array[y, x, :], polygon_color)" works 6 times slower 
                if((img_array[y][x][0] == polygon_color_red) and (img_array[y][x][1] == polygon_color_green) and (img_array[y][x][2] == polygon_color_blue)):
                    label_mask.append((y,x))
                    
        return label_mask


def get_region_masks(img_border_masks):
    # returns pixels of demanded region

    region_masks = []
    for border_mask in img_border_masks:
        region_masks.append(get_region_mask(border_mask))
    return region_masks


def get_region_mask(img_border_mask):
    # considers for each pixel the pixel below. If it is inside the borders then it is added to the set_mask

    set_mask = set(img_border_mask)
    queue_mask = queue.Queue()
    for mask in img_border_mask:
        queue_mask.put(mask)

    while(queue_mask.empty() == False):
        (y,x) = queue_mask.get()
        # check if the pixel below is already in set
        if((y+1,x) in set_mask):
            continue
        # check if the current pixel is not the bottom border: if it is not then there will be a pixel in set with coordinates (y+1, x-1)
        if((y+1,x-1) in set_mask):
            set_mask.add((y+1, x))
            queue_mask.put((y+1, x))

    return list(set_mask)


def combine_dicom_array_and_region_masks(label, dicom_array, img_region_masks):
    # feacture includes label and dcm_color
    features = []
    for i in range (0, len(img_region_masks)):
        for (y,x) in img_region_masks[i]:
            features.append((label,dicom_array[i][y][x]))
    return features

def build_model(features_combined):
    # uses GradientBoostingClassifier

    features_count = len(features_combined)
    atribut_count = len(features_combined[0])-1
    features = np.zeros((features_count)*(atribut_count)).reshape((features_count),(atribut_count))
    target = np.zeros(features_count)
    for i in range(features_count):
        # element 0 has the label
        target[i] = features_combined[i][0] 
        for j in range(atribut_count):
            features[i] = features_combined[i][j+1]

    gbrt = GradientBoostingClassifier(random_state=0, max_depth=4, learning_rate = 0.8)
    gbrt.fit(features, target)

    return gbrt


def build_features_graph(features_0, features_1):
    # prepare data
    x_0 = []
    max_x_value = len(features_0)
    for i in range(max_x_value):
        x_0.append(i)

    x_1 = []
    max_x_value = len(features_1)
    for i in range(max_x_value):
        x_1.append(i)

    y_0 = []
    for feature in features_0:
        y_0.append(feature[1])

    y_1 = []
    for feature in features_1:
        y_1.append(feature[1])

    line_0, line_1 = plt.plot(x_0, y_0, 'bD:', x_1, y_1, 'r^:')

    plt.xlabel('Номер экземпляра')
    plt.ylabel('Цвет')
    plt.legend((line_0, line_1), ('Тип 0', 'Тип 1'), loc = 'best')
    plt.grid()
    plt.savefig('graph.png', format = 'png')

    return


def build_model_main():
    # builds model that will be used to make predictions
    # uses dicom files from folder "files", 
    # images with regions circled in rectangles: folder "img_1" is for images with diseased regions,
    # folder "img_0" is for images with health regions
    
    dcm_path = "files"
    dicom_array = get_dicom_array(dcm_path)
    
    # get features for type 1
    label_1 = 1
    path_1_examples = "img_1"
    img_border_masks_1 = get_border_masks(path_1_examples)
    img_region_masks_1 = get_region_masks(img_border_masks_1)

    features_1 = combine_dicom_array_and_region_masks(label_1, dicom_array, img_region_masks_1)

    # get features for type 0
    label_0 = 0
    path_0_examples = "img_0"
    img_border_masks_0 = get_border_masks(path_0_examples)
    img_region_masks_0 = get_region_masks(img_border_masks_0)

    features_0 = combine_dicom_array_and_region_masks(label_0, dicom_array, img_region_masks_0)

    # build graph to show dependencies
    build_features_graph(features_0, features_1)

    # combine features 1 and 0 to create learn data
    features_1 = np.array(features_1)
    features_0 = np.array(features_0)
    features_combined = np.concatenate((features_0, features_1))

    # build model that makes predictions
    model = build_model(features_combined)

    return model


### the next is for making predictions

def color_image_from_coordinate_list(image_path_from,
                                     image_path_to,
                                     coordinate_list):
    # -coordinate_list is the list of tuples (y,x) that are considered as diseased

    im_origin = Image.open(image_path_from)
    im = Image.new('RGB', im_origin.size)
    pix = im.load()
    coordinates_number = len(coordinate_list)
    # color image
    for i in range(0, coordinates_number):
        y = int(coordinate_list[i][0])
        x = int(coordinate_list[i][1])
        pix[x,y] = (0,200,0)
    # add transparence to an image
    im.putalpha(150)
    # combine original image and transparent image
    im_origin.paste(im.convert('RGB'), (0,0), im)
    im_origin.save(image_path_to)
    im_origin.close()
    im.close()


def color_images_from_coordinate_list(images_path_from,
                                    images_path_to,
                                    coordinate_list):
    images = os.listdir(images_path_from)
    images_number = len(images)
    for i in range(0, images_number):
        #if(len(coordinate_list[i]) == 0):
        #    copyfile(images_path_from + '/' + images[i], images_path_to + '/colored_' + images[i])
        #    continue
        color_image_from_coordinate_list(images_path_from + '/' + images[i],
                                        images_path_to + '/colored_' + images[i],
                                        coordinate_list[i])


def make_prediction(model, dicom_array, lungs_mask):
    # returns for each slice tuples (y,x) which belong to lungs and are marked as diseased
    # uses model to make predictions for dicom_array
    # lungs_masks show if a pixel belongs to lungs
    # uses only one attribure - dicom color of the pixel
    
    slice, height, width = dicom_array.shape
    data_array = np.zeros(dicom_array.size).reshape(dicom_array.size, 1)

    
    for z in range(slice):
        for y in range(height):
            for x in range(width):
                data_array[z+y*slice+x*height*slice] = dicom_array[z][y][x]

    pred = model.predict(data_array)

    cnt = 0
    cnt_all = 0
    for pr in pred:
        if(pr==1):
            cnt = cnt+1
        cnt_all = cnt_all + 1
    print(cnt)
    print(cnt_all)

    #reshape predictions to the shape (slice, []) so for each slice there will be tuples (y,x) as coordinates of diseased pixels
    pred_array = []
    for i in range(slice):
        pred_array.append([])

    for z in range(slice):
        for y in range(height):
            for x in range(width):
                if((lungs_mask[z][y][x] == 1) and (pred[z+y*slice+x*height*slice] == 1)):
                    pred_array[z].append((y,x))

    return pred_array

def make_prediction_boundaries(dicom_array, lungs_mask):
    # returns for each slice tuples (y,x) which belong to lungs and are marked as diseased
    # uses botton boundary of color in dicom
    # lungs_masks show if a pixel belongs to lungs
    # uses only one attribure - dicom color of the pixel
    
    slice, height, width = dicom_array.shape

    #reshape predictions to the shape (slice, []) so for each slice there will be tuples (y,x) as coordinates of diseased pixels
    pred_array = []
    for i in range(slice):
        pred_array.append([])

    for z in range(slice):
        for y in range(height):
            for x in range(width):
                if((lungs_mask[z][y][x] == 1) and (dicom_array[z][y][x] >= 900)):
                    pred_array[z].append((y,x))

    return pred_array


def make_prediction_boundaries_for_coordinate_list(dicom_array, coordinate_list):
    # returns for each slice array [height*width] with 1 if diseased, 0 if is ok and -1 if not in prediction
    # uses botton boundary of color in dicom
    # lungs_masks show if a pixel belongs to lungs
    # uses only one attribure - dicom color of the pixel
    
    slice, height, width = dicom_array.shape

    #reshape predictions to the shape (slice, []) so for each slice there will be tuples (y,x) as coordinates of diseased pixels
    pred_array = np.ndarray(dicom_array.shape)
    pred_array.fill(-1)

    for z in range(slice):
        for (y,x) in coordinate_list[z]:
            if(dicom_array[z][y][x] >= 900):
                pred_array[z][y][x] = 1
            else:
                pred_array[z][y][x] = 0

    return pred_array

def get_actual_values_for_images(img_region_masks_1, img_region_masks_0, height, width):
    # returns mask: 1,0,-1 (-1 if there is no info)

    actual_array = np.ndarray(len(img_border_masks_0)*width*height).reshape(len(img_border_masks_0), height, width)
    actual_array.fill(-1)

    slice = len(img_border_masks_0)
    for z in range(slice):
        for (y,x) in img_region_masks_1[z]:
            actual_array[z][y][x] = 1
        for (y,x) in img_region_masks_0[z]:
            actual_array[z][y][x] = 0

    return actual_array
        


def compare_actual_and_predicted(actual_array, prediction_array):
    # in result each pixel has value:
    # -1 - not in work
    # 0 - both 0 (true negative tn); 
    # 1 - both 1 (true predicted tp); 
    # 2 - actual 0, predicted 1 (false predicted fp); 
    # 3 - actual 1, predicted 0 (false negative fn)

    slice, height, width = actual_array.shape
    difference_array = np.ndarray(actual_array.shape)
    difference_array.fill(-1)

    for z in range(slice):
        for y in range(height):
            for x in range(width):
                if((actual_array[z][y][x] == 0) and (prediction_array[z][y][x] == 0)):
                    difference_array[z][y][x] = 0
                if((actual_array[z][y][x] == 1) and (prediction_array[z][y][x] == 1)):
                    difference_array[z][y][x] = 1
                if((actual_array[z][y][x] == 0) and (prediction_array[z][y][x] == 1)):
                    difference_array[z][y][x] = 2
                if((actual_array[z][y][x] == 1) and (prediction_array[z][y][x] == 0)):
                    difference_array[z][y][x] = 3

    return difference_array


def count_actual_and_predicted(path_to_save, difference_array):

    # 0 - both 0 (true negative tn); 
    # 1 - both 1 (true predicted tp); 
    # 2 - actual 0, predicted 1 (false predicted fp); 
    # 3 - actual 1, predicted 0 (false negative fn)
    count_0 = 0
    count_1 = 0
    count_2 = 0
    count_3 = 0
    
    slice, height, width = difference_array.shape

    for z in range(slice):
        for y in range(height):
            for x in range(width):
                if(difference_array[z][y][x] == 0):
                    count_0 += 1
                if(difference_array[z][y][x] == 1):
                    count_1 += 1
                if(difference_array[z][y][x] == 2):
                    count_2 += 1
                if(difference_array[z][y][x] == 3):
                    count_3 += 1

    with open(path_to_save,'w') as csv_file:
        csv_file.write('tp;fp;fn;tn\n')
        res = str(count_1) + ';' + str(count_2) + ';' + str(count_3) + ';' + str(count_0) + '\n'
        csv_file.write(res)

    return


def color_actual_and_predicted(image_path_from, image_path_to, difference_array):
    
    images = os.listdir(image_path_from)
    images_number = len(images)
    for i in range(0, images_number):
        color_actual_and_predicted_for_slice(image_path_from + '/' + images[i],
                                image_path_to + '/colored_' + images[i],
                                difference_array[i])
    return


def color_actual_and_predicted_for_slice(image_path_from, image_path_to, difference_array):
    # 0 - both 0 (true negative tn); 
    # 1 - both 1 (true predicted tp); 
    # 2 - actual 0, predicted 1 (false predicted fp); 
    # 3 - actual 1, predicted 0 (false negative fn)
    
    im_origin = Image.open(image_path_from)
    im = Image.new('RGB', im_origin.size)
    pix = im.load()

    height, width = difference_array.shape

    for y in range(height):
        for x in range(width):
            if(difference_array[y][x] == 0):
                pix[x,y] = (0,0,200)
            if(difference_array[y][x] == 1):
                pix[x,y] = (0,200,0)
            if(difference_array[y][x] == 2):
                pix[x,y] = (200,0,200)
            if(difference_array[y][x] == 3):
                pix[x,y] = (200,0,0)
            
    # add transparence to an image
    im.putalpha(150)
    # combine original image and transparent image
    im_origin.paste(im.convert('RGB'), (0,0), im)
    im_origin.save(image_path_to)
    im_origin.close()
    im.close()

    return


def color_bmp_from_dcm_file(image_path_from, image_path_to, dicom_array, lungs_mask):
    
    im_origin = Image.open(image_path_from)
    im = Image.new('RGB', im_origin.size)
    pix = im.load()

    height, width = dicom_array.shape

    for y in range(height):
        for x in range(width):
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 900) and (dicom_array[y][x] < 950)):
                pix[x,y] = (222,154,29)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 950) and (dicom_array[y][x] < 1000)):
                pix[x,y] = (222,218,28)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1000) and (dicom_array[y][x] < 1050)):
                pix[x,y] = (106,222,29)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1050) and (dicom_array[y][x] < 1100)):
                pix[x,y] = (28,223,189)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1100) and (dicom_array[y][x] < 1150)):
                pix[x,y] = (30,125,221)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1150) and (dicom_array[y][x] < 1200)):
                pix[x,y] = (35,31,220)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1200) and (dicom_array[y][x] < 1250)):
                pix[x,y] = (107,33,218)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1250) and (dicom_array[y][x] < 1300)):
                pix[x,y] = (214,33,218)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1350) and (dicom_array[y][x] < 1400)):
                pix[x,y] = (222,29,29)
            if((lungs_mask[y][x] == 1) and (dicom_array[y][x] >= 1400)):
                pix[x,y] = (136,115,134)

    # add transparence to an image
    im.save("test_image_colors.bmp")
    im.putalpha(150)
    # combine original image and transparent image
    im_origin.paste(im.convert('RGB'), (0,0), im)
    im_origin.save(image_path_to)
    im_origin.close()
    im.close()

    return


def color_bmp_from_dcm_file_list(images_path_from,
                            images_path_to,
                            dicom_array,
                            lungs_mask):
    images = os.listdir(images_path_from)
    images_number = len(images)
    for i in range(0, images_number):
        color_bmp_from_dcm_file(images_path_from + '/' + images[i],
                                images_path_to + '/colored_' + images[i],
                                dicom_array[i],
                                lungs_mask[i])


def analize2():
    print("read")

    # get test dicoms 
    dcm_path = "files_test"
    dicom_test_array = get_dicom_array(dcm_path)
    
    print("convert")

    #convert dicom to bmp
    path_img_pred = "img_test"
    read_dicoms_to_images(dcm_path, 'lung', path_img_pred, 0, 0, False, False)

    print("lungs_mask")

    # get mask if a pixel is for lungs
    lungs_mask = get_lungs_masks_for_slices(dicom_test_array)
    #lungs_mask = remove_borders_from_lungs_mask(lungs_mask, dicom_test_array)

    #print("color_lungs_mask")

    #color images with lung_label_mask
    #path_colored_lungs_mask = "label_masks_colored2"
    #color_label_mask_for_slices(lungs_mask, path_img_pred, path_colored_lungs_mask)

    print("lungs_mask_pseudo")

    #lungs_mask_temp = np.zeros(lungs_mask.shape)
    #sli,hi,wi = lungs_mask.shape
    #for z in range(sli):
    #    for y in range(hi):
    #        for x in range(wi):
    #            lungs_mask_temp[z][y][x] = lungs_mask[z][y][x]

    lungs_mask = remove_pseudo_lungs_mask(lungs_mask, dicom_test_array)
    
    print("color_lungs_mask_pseudo")

    #color images with lung_label_mask
    path_colored_lungs_mask = "label_masks_colored"
    color_label_mask_for_slices(lungs_mask, path_img_pred, path_colored_lungs_mask)

    #print("prediction1")


    #im_origin = Image.open("images_for_paper/1.bmp")
    #im = Image.new('RGB', im_origin.size)
    #pix = im.load()
    
    #for y in range(512):
    #    for x in range(512):
    #        if(lungs_mask[0][y][x]==1):
    #            pix[x,y] = (0,200,0)
    #        if(lungs_mask[0][y][x]==0) and (lungs_mask_temp[0][y][x]==1):
    #            pix[x,y] = (0,90,255)

    ## add transparence to an image
    #im.putalpha(150)
    ## combine original image and transparent image
    #im_origin.paste(im.convert('RGB'), (0,0), im)
    #im_origin.save("images_for_paper/2.bmp")
    #im_origin.close()
    #im.close()

    ## make prediction 
    #prediction = make_prediction(model, dicom_test_array, lungs_mask)


    ## color bmp images using prediction
    #path_colored = "colored_img"
    #color_images_from_coordinate_list(path_img_pred, path_colored, prediction)

    print("prediction2")

    #### model without AI, just bounders for color
    prediction_boundaries = make_prediction_boundaries(dicom_test_array, lungs_mask)

    # color bmp images using prediction
    path_colored = "colored_img/boundaries"
    color_images_from_coordinate_list(path_img_pred, path_colored, prediction_boundaries)
    return


def remove_temp_files(path):

    images = os.listdir(path)
    images_number = len(images)
    for i in range(0, images_number):
        os.remove(path + '/' + images[i])
    return


def analize(dcm_path, path_colored):

    # get test dicoms 
    dicom_test_array = get_dicom_array(dcm_path)
    
    #convert dicom to bmp
    path_img_pred = "img_test"
    read_dicoms_to_images(dcm_path, 'lung', path_img_pred, 0, 0, False, False)


    # get mask if a pixel is for lungs
    lungs_mask = get_lungs_masks_for_slices(dicom_test_array)
    lungs_mask = remove_pseudo_lungs_mask(lungs_mask, dicom_test_array)
    
    #### model without AI, just bounders for color
    prediction_boundaries = make_prediction_boundaries(dicom_test_array, lungs_mask)

    # color bmp images using prediction
    color_images_from_coordinate_list(path_img_pred, path_colored, prediction_boundaries)

    remove_temp_files(path_img_pred);

    return


def paper_things():
    #model = build_model_main()

    # analize()


    # build confusion matrix (photos)

    # get dicoms 
    dcm_path = "img_comparing/files"
    dicom_array = get_dicom_array(dcm_path)

    # read image with colored regions
    img_path = "img_comparing/img"

    img_border_masks_1 = get_border_masks(img_path)
    img_region_masks_1 = get_region_masks(img_border_masks_1)
    img_border_masks_0 = get_border_masks(img_path, (34,177,76))
    img_region_masks_0 = get_region_masks(img_border_masks_0)
    
    # get actual values
    slice, height, width = dicom_array.shape
    actual_array = get_actual_values_for_images(img_region_masks_1,img_region_masks_0, height, width)

    # predict values
    img_region_masks_combined = []
    for z in range(slice):
        if((len(img_region_masks_1[z])>0) and (len(img_region_masks_0[z])>0)):
            img_region_masks_combined.append(np.concatenate((img_region_masks_1[z], img_region_masks_0[z])))
        else:
           if(len(img_region_masks_1[z])>0):
                img_region_masks_combined.append(img_region_masks_1[z])
           else:
                img_region_masks_combined.append(img_region_masks_0[z])
    
    prediction_array = make_prediction_boundaries_for_coordinate_list(dicom_array, img_region_masks_combined)

    # compare actual and predicted
    difference_array = compare_actual_and_predicted(actual_array, prediction_array)
    img_path_from_for_result = "img_comparing/origin"
    img_path_result = "img_comparing/res"
    color_actual_and_predicted(img_path_from_for_result, img_path_result, difference_array)
    count_path_result = "img_comparing/res.csv"
    count_actual_and_predicted(count_path_result, difference_array)



    ##color bmp images using dcm color
    #path_color_mask = "colored_img/mask"
    #color_bmp_from_dcm_file_list(path_img_pred, path_color_mask, dicom_test_array, lungs_mask)

    #(colored images, percent of diseased pixels for each slice)

    print("ok")


if __name__ == '__main__':
   analize()
    
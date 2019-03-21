import dicom
import os

from PIL import Image
from config.dicom_working_config import dicom_options as d_opt


def save_slices_as_images(dicom_slices, folder_path_to_save):
    """
    Saves dicom slices pixel data as a set of images in the folder
    :param dicom_slices: ndarray of dicom slices pixel data
    :param folder_path_to_save: Path to the folder to save dicom images
    """
    for i in range(dicom_slices.shape[0]):
        ct_image = Image.fromarray(dicom_slices[i]).convert('RGB')
        ct_image_path = os.path.join(folder_path_to_save,
                                     d_opt['filename_pattern'].format(i))
        ct_image.save(ct_image_path)


def save_ct_images(images, folder_path_to_save):
    """
    Saves ready slice images in the folder
    :param images: List of PIL Image objects
    :param folder_path_to_save: Path to the folder to save
    """
    for i in range(len(images)):
        image_path = os.path.join(folder_path_to_save,
                                  d_opt['filename_pattern'].format(i))
        images[i].save(image_path)


def save_dicom_data(dicom_file_data, file_path_to_save, with_original_meta=True):
    """
    Saves dicom data as a dicom file
    :param dicom_file_data: Dicom data like pydicom.dataset.FileDataset
    :param file_path_to_save: Full path to the file to save as a dicom file
    :param with_original_meta: Whether to save new file with original metadata.
    If original file does not have metadata writes None
    """
    dicom.write_file(file_path_to_save,
                     dicom_file_data,
                     write_like_original=with_original_meta)

import numpy as np

from config import radiology_tissue_options as rt_opt


def get_bone_mask(dicom_slices):
    """
    Returns a mask based on the tissue density from dicom data where
    0 corresponds to tissues which density is lower than bone
    1 corresponds to tissues which density is equal or higher than bone
    :param dicom_slices: ndarray of dicom slices data
    :return: ndarray of masks for slices
    """
    mask = np.copy(dicom_slices)
    left = rt_opt['bone_dicom']['left']
    right = rt_opt['bone_dicom']['right']

    mask[mask < left] = 0
    mask[mask > right] = 0
    mask[mask != 0] = 1

    return mask

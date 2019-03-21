"""
This file contains constants for feature extraction
"""
import numpy as np

load_features_config = {
    'dicom_folder_name': 'dicom-data',
    'label_folder_name': 'label-masks',
    'label_file_extensions': [
        'npy'
    ]
}

save_features_config = {
    'save_modes': {
        'single': 1,
        'multi': 2
    },
    'features_file_name': 'features.csv',
    'slice_sampling_to_save': 10,
    'features_dir_name': 'features',
    'features_index_file_name': 'features_index.csv',
    'features_meta_classes_num_key': 'classes_number',
    'features_meta_file_name': 'features_meta.json'
}

glcm_feature_options = {
    'metadata_num': 6,
    'distances': [
        1
    ],
    'angles': [
        0,
        np.pi/4,
        np.pi/2,
        3*np.pi/4
    ]
}

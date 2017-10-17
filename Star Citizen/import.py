#
# Plan 1: Automated ship model download and processing
#   1) Download Fleet VieweR Star Citizen Ships 3D Models - Data as csv
#   2) For each "Download Model Path Remote"
#   2.1) Download .ctm file
#   2.2) Read MeshLab settings and determine which "original_to_LOD0.mlx" to use
#   2.3) meshlabserver.exe -i filename.ctm -o filename_fv_LOD0.obj -s original_to_LOD0.mlx
#      Transform: Scale, Normalize: Scale to Unit bbox true
#      Remove Duplicate Vertices
#      Remove Unreferenced Vertices
#      Re-Compute Face Normals
#      [Optional] Invert Faces Orientation
#   2.4) meshlabserver.exe -i filename_fv_LOD0.obj -o filename_fv_LOD1.obj -s LOD0_to_LOD1.mlx
#      Ambient Occlusion
#      Select by Vertext Quality: min:0, max:0.0001
#      Delete Selected Faces
#      Remove Unreferenced Vertices
#   3) 
#

spreadsheetId = '1ammOh0-6BHe2hYFQ5c1pdYrLlNjVaphd5bDidcdWqRo'
sheetId = 0
FIELD_MODEL_PATH_REMOTE = 'Model Path Remote'
FIELD_MESHLAB_ROTATION = 'MeshLab\nRotation'
FIELD_MESHLAB_INVERT_NORMALS = 'MeshLab\nInvert Normals'

import requests
from cachecontrol import CacheControl
from cachecontrol.caches.file_cache import FileCache
session = CacheControl(requests.session(), cache=FileCache('.cache'))
response = session.get('https://docs.google.com/spreadsheets/d/%s/export?gid=%d&format=csv' % (spreadsheetId, sheetId))
response.raise_for_status()

import csv
import io
import os
with io.StringIO(response.text) as buff:
  reader = csv.DictReader(buff)
  for line in reader:
    pathRemote = line[FIELD_MODEL_PATH_REMOTE]
    print('          pathRemote:%r' % pathRemote)
    meshlabRotation = line[FIELD_MESHLAB_ROTATION]
    print('     meshlabRotation:%r' % meshlabRotation)
    meshlabInvertNormals = line[FIELD_MESHLAB_INVERT_NORMALS]
    print('meshlabInvertNormals:%r' % meshlabInvertNormals)
    if not pathRemote:
      continue
    response = session.get(pathRemote)
    response.raise_for_status()

    filename = os.path.splitext(os.path.split(pathRemote)[1])[0]
    filename_ctm = filename + '.ctm'
    filename_obj = filename + '.obj'
    with open(filename_ctm, 'wb') as handle:
      for block in response.iter_content(1024):
        handle.write(block)

#!/usr/bin/env python3
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
FIELD_MESHLAB_FORWARD_UP_NORMAL = 'MeshLab\nForward_Up_Normal'
MESHLABSERVER_EXE = 'c:\\Program Files\\VCG\\MeshLab\\meshlabserver.exe'
# MacOS required hack:
# $ cd /Applications/meshlab.app/Contents/MacOS/
# $ install_name_tool -add_rpath "@executable_path/../Frameworks" meshlabserver
#MESHLABSERVER_EXE = '/Applications/meshlab.app/Contents/MacOS/meshlabserver'


import requests
from cachecontrol import CacheControl
from cachecontrol.caches.file_cache import FileCache
import csv
import io
import os
import subprocess

def execute(cmd_args, cwd=None, check=True):
  print('execute(%r)' % cmd_args)
  with subprocess.Popen(cmd_args, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as p:
    output, errors = p.communicate()
  print(output.decode("utf-8"))
  return p.returncode

def main():
  session = CacheControl(requests.session(), cache=FileCache('.cache'))
  response = session.get('https://docs.google.com/spreadsheets/d/%s/export?gid=%d&format=csv' % (spreadsheetId, sheetId))
  response.raise_for_status()
  #print(response.content)

  processed = []

  with io.StringIO(response.text) as buff:

    rows = csv.DictReader(buff)

    for row in rows:
      #print(row)

      pathRemote = row[FIELD_MODEL_PATH_REMOTE]
      print('                 pathRemote:%r' % pathRemote)
      meshlabForwardUpNormal = row[FIELD_MESHLAB_FORWARD_UP_NORMAL]
      if not meshlabForwardUpNormal:
        meshlabForwardUpNormal = 'nZ_pY_Normal'
      print('     meshlabForwardUpNormal:%r' % meshlabForwardUpNormal)
      
      if not pathRemote or pathRemote in processed:
        continue

      filename_ctm = os.path.split(pathRemote)[1]
      filename = os.path.splitext(filename_ctm)[0]

      response = session.get(pathRemote, stream=True)
      print('response.status_code:%d' % response.status_code)
      response.raise_for_status()

      with open(filename_ctm, 'wb') as handle:
        for block in response.iter_content(chunk_size=1024):
          if block:
            print(".", end="")
            handle.write(block)
        handle.flush()
      print("")

      meshlab_script = 'LOD0_%s.mlx' % meshlabForwardUpNormal
      filename_lod0_obj = filename + '_fv_LOD0.obj'
      cmd_args = [MESHLABSERVER_EXE, "-i", filename_ctm, "-o", filename_lod0_obj, '-s', meshlab_script]
      if execute(cmd_args):
        break

      meshlab_script = 'LOD1.mlx'
      filename_lod1_obj = filename + '_fv_LOD1.obj'
      cmd_args = [MESHLABSERVER_EXE, "-i", filename_lod0_obj, "-o", filename_lod1_obj, '-s', meshlab_script]
      if execute(cmd_args):
        break
      
      processed.append(pathRemote)

      #break
    
if __name__ == '__main__':
  main()

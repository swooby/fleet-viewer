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
#
# Requires MeshLab installed
# http://www.meshlab.net/#download
#
# Required Blender Setup:
#   cd /your/path/to/blender/python/bin
#   curl -O https://bootstrap.pypa.io/get-pip.py
#   chmod +x get-pip.py
#   ./python3.5m get-pip.py
#   ./python3.5m pip install requests
#   ./python3.5m pip install cachecontrol
#   ./python3.5m pip install lockfile
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

import bpy
import csv
import io
import os
import subprocess
import time
import requests
from cachecontrol import CacheControl
from cachecontrol.caches.file_cache import FileCache


def execute(cmd_args, cwd=None):
  print('execute(%r)' % cmd_args)
  with subprocess.Popen(cmd_args, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as p:
    output, errors = p.communicate()
  print(output.decode("utf-8"))
  return p.returncode


class RsiCtmToLods:
  def setLayers(self, scene, obj, *indices):
    obj.layers = [ i in indices for i in range(len(obj.layers)) ]

  def decimate(self, scene, obj, ratio):
    if obj.type != "MESH":
      return
    modDec = obj.modifiers.new("Decimate", type = "DECIMATE")
    modDec.ratio = ratio
    scene.update()

  def selectLayers(self, scene, layers):
    for i in range(len(scene.layers)):
      scene.layers[i] = layers[i]

  def selectObject(self, scene, obj):
    for obj2 in scene.objects:
      obj2.select = obj2 == obj

  def importAndProcessAll(self):
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

        self.downloadCtm(session, pathRemote)

        filename_ctm = os.path.split(pathRemote)[1]
        filename = os.path.splitext(filename_ctm)[0]

        self.meshlabProcessCtmToObj(filename, meshlabForwardUpNormal)
        
        self.blenderProcessObj(filename)

        self.blenderExportScene()

        processed.append(pathRemote)

  def downloadCtm(self, session, pathRemote):
    print("Downloading %r" % pathRemote)
    
    filename_ctm = os.path.split(pathRemote)[1]
    filename = os.path.splitext(filename_ctm)[0]

    # TODO:(pv) Don't bother downloading or processing if response.status == 304

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

  def meshlabProcessCtmToObj(self, filename, meshlabForwardUpNormal):
    print("%r MeshLab CTM to OBJ" % filename)
    
    #
    # TODO:(pv) Consider using https://github.com/3DLIRIOUS/MeshLabXML
    #

    filename_ctm = filename + ".ctm"

    meshlab_script = 'LOD0_%s.mlx' % meshlabForwardUpNormal
    filename_lod0_obj = filename + '_fv_LOD0.obj'
    cmd_args = [MESHLABSERVER_EXE, "-i", filename_ctm, "-o", filename_lod0_obj, '-s', meshlab_script]
    execute(cmd_args)

    meshlab_script = 'LOD1.mlx'
    filename_lod1_obj = filename + '_fv_LOD1.obj'
    cmd_args = [MESHLABSERVER_EXE, "-i", filename_lod0_obj, "-o", filename_lod1_obj, '-s', meshlab_script]
    execute(cmd_args)

  def blenderProcessObj(self, filename):
    print("%r Blender OBJ LODs" % filename)
    bpy.ops.wm.read_homefile(use_empty=True)
    scene = bpy.context.scene
    obj_lod0 = self.loadAndSetLodRatio(scene, filename, 0, None)
    obj_lod1 = self.loadAndSetLodRatio(scene, filename, 1, 0.90)
    obj_lod2 = self.copyAndSetLodRatio(scene, obj_lod1, 2, 0.60)
    obj_lod3 = self.copyAndSetLodRatio(scene, obj_lod1, 3, 0.30)
    obj_lod4 = self.copyAndSetLodRatio(scene, obj_lod1, 4, 0.10)
    obj_lod5 = self.copyAndSetLodRatio(scene, obj_lod1, 5, 0.05)
    bpy.ops.wm.save_as_mainfile(filepath=filename + ".blend", check_existing=False)

  def loadAndSetLodRatio(self, scene, filename, lod, ratio):
    obj_lodX_name = filename + "_fv_LOD%d" % lod
    filename_lodX = obj_lodX_name + ".obj"
    print("Blender Load %r " % filename_lodX)
    #
    # https://docs.blender.org/api/current/bpy.ops.import_scene.html#bpy.ops.import_scene.obj
    #
    bpy.ops.import_scene.obj(filepath=filename_lodX)
    obj_lodX = scene.objects[obj_lodX_name]
    self.setLayers(scene, obj_lodX, lod)
    if ratio:
      self.decimate(scene, obj_lodX, ratio)
    return obj_lodX

  def copyAndSetLodRatio(self, scene, obj, lod, ratio):
    obj_lodX_name = obj.name[:obj.name.rfind("_fv_LOD")] + "_fv_LOD%d" % lod
    print("Blender Create %r " % obj_lodX_name)
    obj_lodX = obj.copy()
    obj_lodX.data = obj.data.copy()
    obj_lodX.animation_data_clear()
    scene.objects.link(obj_lodX)
    obj_lodX.name = obj_lodX_name
    obj_lodX.data.name = obj_lodX_name
    self.setLayers(scene, obj_lodX, lod)
    self.decimate(scene, obj_lodX, ratio)
    return obj_lodX

  def blenderExportScene(self, scene=None):
    if not scene:
      scene = bpy.context.scene
    for obj in scene.objects:
      self.blenderExportObject(obj, scene)

  def blenderExportName(self, name, scene=None):
    if not scene:
      scene = bpy.context.scene
    self.blenderExportObject(scene.objects[name], scene)

  def blenderExportObject(self, obj, scene=None):
    if not scene:
      scene = bpy.context.scene
    print("Blender Export %r" % obj.name)
    self.selectLayers(scene, obj.layers)
    self.selectObject(scene, obj)

    blend_file_dir = os.path.dirname(bpy.data.filepath)
    filepath = os.path.join(blend_file_dir, obj.name)
    
    #
    # https://docs.blender.org/api/current/bpy.ops.export_scene.html#bpy.ops.export_scene.obj
    #
    bpy.ops.export_scene.obj(filepath=filepath + ".obj", check_existing=False, use_selection=True)
    
    self.obj2ctm(filepath)

  def obj2ctm(self, filepath):
    print("Export %r to CTM" % filepath)
    if True:
      #
      # Requires MeshLab installed
      # http://www.meshlab.net/#download
      #
      # This seems like a win/win that produces the best quality output *AND* smallest files
      #
      
      cwd = os.path.dirname(filepath) or None
      filename = os.path.split(filepath)[1]
      filename_obj = filename + ".obj"
      filename_ctm = filename + ".ctm"

      cmd_args = [MESHLABSERVER_EXE, "-i", filename_obj, "-o", filename_ctm]
      execute(cmd_args, cwd=cwd)
    else:
        
      filepath_obj = filepath + ".obj"
      filepath_ctm = filepath + ".ctm"
      
      if True:
        #
        # Requires OpenCTM SDK installed
        # http://openctm.sourceforge.net/?page=download
        # https://sourceforge.net/projects/openctm/files/OpenCTM-1.0.3/OpenCTM-1.0.3-setup.exe/download
        #
        # ERROR: The output of this command looks horrible in Unity!
        #
        cmd_args = ["ctmconv", filepath_obj, filepath_ctm, "--method", "MG1", "--level", "6"]
        execute(cmd_args)
      else:
        #
        # Requires special 64-bit build of openctm.dll placed in Blender home directory
        # https://sourceforge.net/projects/openctm/files/
        # https://sourceforge.net/projects/openctm/files/OpenCTM-1.0.3/OpenCTM-1.0.3-src.zip/download
        #
        # ERROR: Can't get this to work
        #
      
        filepath_obj = ctypes.c_char_p(filepath_obj.encode("utf-8"))
        filepath_ctm = ctypes.c_char_p(filepath_ctm.encode("utf-8"))
      
        try:
          ctmIn = ctmNewContext(CTM_IMPORT)
          ctmLoad(ctmIn, filepath_obj)
          vertCount = ctmGetInteger(ctmIn, CTM_VERTEX_COUNT)
          verts = ctmGetFloatArray(ctmIn, CTM_VERTICES)
          triCount = ctmGetInteger(ctmIn, CTM_TRIANGLE_COUNT)
          indices = ctmGetIntegerArray(ctmIn, CTM_INDICES)
        finally:
          ctmFreeContext(ctmIn)
  
        try:    
          ctmOut = ctmNewContext(CTM_EXPORT)
          ctmDefineMesh(ctmOut, verts, vertCount, indices, triCount, None)
          ctmCompressionMethod(ctmOut, CTM_METHOD_MG1)
          #ctmCompressionLevel(ctmOut, 9)
          ctmSave(ctmOut, filepath_ctm)
        finally:
          ctmFreeContext(ctmOut)

def main():
  rsiCtmToLods = RsiCtmToLods()
  rsiCtmToLods.importAndProcessAll()
  
if __name__ == '__main__':
  main()

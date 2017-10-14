import bpy
import os
import subprocess
import time
from openctm import *
import ctypes


class CtmExporter:
  def __init__(self, blender):
    self.scene = blender.context.scene
    self.blend_file_dir = os.path.dirname(blender.data.filepath)

  def exportAll(self):
    for obj in self.scene.objects:
      self.exportObject(obj)

  def exportName(self, name):
    self.exportObject(self.scene.objects[name])

  def exportObject(self, obj):
    self.selectLayers(obj.layers)
    self.selectObject(obj)
    
    filepath = os.path.join(self.blend_file_dir, obj.name)
    
    #
    # https://docs.blender.org/api/current/bpy.ops.export_scene.html#bpy.ops.export_scene.obj
    #
    bpy.ops.export_scene.obj(filepath=filepath + ".obj", check_existing=False, use_selection=True)
    
    self.obj2ctm(filepath)

  def selectLayers(self, layers):
    for i in range(len(self.scene.layers)):
      self.scene.layers[i] = layers[i]

  def selectObject(self, object):
    for obj in self.scene.objects:
      obj.select = False
    object.select = True

  def obj2ctm(self, filepath):
    if True:
      #
      # Requires MeshLab installed
      # http://www.meshlab.net/#download
      #
      # This is a win/win that produces the best quality output and smallest files
      #
      
      cwd = os.path.dirname(filepath)
      filename = os.path.split(filepath)[1]
      filename_obj = filename + ".obj"
      filename_ctm = filename + ".ctm"

      cmd_args = ["c:\\Program Files\\VCG\\MeshLab\\meshlabserver.exe", "-i", filename_obj, "-o", filename_ctm]
      with subprocess.Popen(cmd_args, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as p:
        output, errors = p.communicate()
      print(output.decode("utf-8"))
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
        with subprocess.Popen(cmd_args, stdout=subprocess.PIPE, stderr=subprocess.PIPE) as p:
          output, errors = p.communicate()
        print(output.decode("utf-8"))
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
  exporter = CtmExporter(bpy)
  exporter.exportAll()
  #exporter.exportName("Idris_cleaned_transformed_fv_LOD5")
  

if __name__ == '__main__':
    main()

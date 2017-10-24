import bpy
import bmesh
from bpy_extras.object_utils import world_to_camera_view
import math
import mathutils
from mathutils import Euler
from mathutils import Matrix
from mathutils import Quaternion
from mathutils import Vector
from mathutils.geometry import normal


def camera_as_planes(scene, obj):
  """
  Return planes in world-space which represent the camera view bounds.
  """
  camera = obj.data
  # normalize to ignore camera scale
  matrix = obj.matrix_world.normalized()
  frame = [matrix * v for v in camera.view_frame(scene)]
  origin = matrix.to_translation()

  planes = []
  is_persp = (camera.type != 'ORTHO')
  for i in range(4):
    # find the 3rd point to define the planes direction
    if is_persp:
      frame_other = origin
    else:
      frame_other = frame[i] + matrix.col[2].xyz

    n = normal(frame_other, frame[i - 1], frame[i])
    d = -n.dot(frame_other)
    planes.append((n, d))

  if not is_persp:
    # add a 5th plane to ignore objects behind the view
    n = normal(frame[0], frame[1], frame[2])
    d = -n.dot(origin)
    planes.append((n, d))

  return planes

def side_of_plane(p, v):
  return p[0].dot(v) + p[1]

def is_segment_in_planes(p1, p2, planes):
  dp = p2 - p1

  p1_fac = 0.0
  p2_fac = 1.0

  for p in planes:
    div = dp.dot(p[0])
    if div != 0.0:
      t = -side_of_plane(p, p1)
      if div > 0.0:
        # clip p1 lower bounds
        if t >= div:
          return False
        if t > 0.0:
          fac = (t / div)
          p1_fac = max(fac, p1_fac)
          if p1_fac > p2_fac:
            return False
      elif div < 0.0:
        # clip p2 upper bounds
        if t > 0.0:
          return False
        if t > div:
          fac = (t / div)
          p2_fac = min(fac, p2_fac)
          if p1_fac > p2_fac:
            return False

  ## If we want the points
  # p1_clip = p1.lerp(p2, p1_fac)
  # p2_clip = p1.lerp(p2, p2_fac)
  return True

def point_in_object(obj, pt):
  xs = [v[0] for v in obj.bound_box]
  ys = [v[1] for v in obj.bound_box]
  zs = [v[2] for v in obj.bound_box]
  pt = obj.matrix_world.inverted() * pt
  return (min(xs) <= pt.x <= max(xs) and
          min(ys) <= pt.y <= max(ys) and
          min(zs) <= pt.z <= max(zs))

def object_in_planes(obj, planes):
  matrix = obj.matrix_world
  box = [matrix * Vector(v) for v in obj.bound_box]
  for v in box:
    if all(side_of_plane(p, v) < 0.0 for p in planes):
      # one point was in all planes
      return True

  # possible one of our edges intersects
  edges = ((0, 1), (0, 3), (0, 4), (1, 2),
           (1, 5), (2, 3), (2, 6), (3, 7),
           (4, 5), (4, 7), (5, 6), (6, 7))
  return any(is_segment_in_planes(box[e[0]], box[e[1]], planes) for e in edges)

def objects_in_planes(objects, planes, origin):
  """
  Return all objects which are inside (even partially) all planes.
  """
  return [obj for obj in objects
          if point_in_object(obj, origin) or
             object_in_planes(obj, planes)]

def select_objects_in_camera(scene, camera=None):
  if not camera:
    camera = scene.camera
  origin = camera.matrix_world.to_translation()
  planes = camera_as_planes(scene, camera)
  objects_in_view = objects_in_planes(scene.objects, planes, origin)
  for obj in objects_in_view:
    obj.select = True

def look_at(obj, point, yaw=None, pitch=None):
  '''
  From https://blender.stackexchange.com/a/5220/47021
  '''
  obj_location = obj.matrix_world.to_translation()
  direction = point - obj_location
  rot_quat = direction.to_track_quat('-Z', 'Y')
  print("look_at: obj.rotation_mode:%r" % obj.rotation_mode)
  rot_euler = rot_quat.to_euler()
  print("look_at: BEFORE rot_euler:%r" % rot_euler)
  if yaw:
    rot_euler.z += yaw
  if pitch:
    rot_euler.x += pitch
  print("look_at: AFTER rot_euler:%r" % rot_euler)
  obj.rotation_euler = rot_euler

def object_as_camera(context, view3dAreaAndRegion, obj):
  if not view3dAreaAndRegion:
    view3dAreaAndRegion = getView3dAreaAndRegion(context)
  view3dArea, view3dRegion = view3dAreaAndRegion
  override = context.copy()
  override['area'] = view3dArea
  override['region'] = view3dRegion
  override['active_object'] = obj
  bpy.ops.view3d.object_as_camera(override)
  return view3dAreaAndRegion

def select_border(context, view3dAreaAndRegion=None, extend=True):
  if not view3dAreaAndRegion:
    view3dAreaAndRegion = getView3dAreaAndRegion(context)
  view3dArea, view3dRegion = view3dAreaAndRegion
  #print("view3dArea.height:%r" % view3dArea.height)
  #print("view3dArea.width:%r" % view3dArea.width)
  override = context.copy()
  override['area'] = view3dArea
  override['region'] = view3dRegion
  bpy.ops.view3d.select_border(override,
                               gesture_mode=3,
                               xmin=0,
                               xmax=view3dArea.width,
                               ymin=0,
                               ymax=view3dArea.height,
                               extend=extend)
  return view3dAreaAndRegion

def getView3dAreaAndRegion(context):
  for area in context.screen.areas:
    if area.type == "VIEW_3D":
      for region in area.regions:
        if region.type == "WINDOW":
          return area, region


class SelectBridge:
  def __init__(self):
    self.context = bpy.context
  
    self.view3dAreaAndRegion = getView3dAreaAndRegion(self.context)
  
    self.scene = self.context.scene

    self.idris = self.scene.objects["Idris_fv_LOD0"]
    print("idris:%r" % self.idris)
    #idris_mesh = idris.data
    #idris_matrix_world = idris.matrix_world

    self.table_camera = self.scene.objects["Table Camera"]
    print("table_camera:%r" % self.table_camera)
    #table_camera.position = (0.114582, -2.26953, 0.738129)
  
    #self.scene.camera = self.table_camera

    self.table_orbit = self.scene.objects["Table Orbit"]
    print("table_orbit:%r" % self.table_orbit)
    self.table_orbit_location = self.table_orbit.location
    print("table_orbit.location:%r" % self.table_orbit_location)
    self.table_orbit_matrix_world = self.table_orbit.matrix_world
    print("table_orbit.matrix_world:%r" % self.table_orbit_matrix_world)
    self.table_orbit_matrix_world_to_translation = self.table_orbit.matrix_world.to_translation()
    print("table_orbit.matrix_world.to_translation():%r" % self.table_orbit_matrix_world_to_translation)

    self.table_orbit_data = self.table_orbit.data
    print("table_orbit.data:%r" % self.table_orbit_data)

    self.vertices = self.table_orbit_data.vertices
    print("vertices[0].co:%r" % self.vertices[0].co)

  def processEdgeIndex(self, edgeIndex=None, yawIndex=None, pitchIndex=None):
    if edgeIndex == None:
      #bpy.context.scene.objects.active = idris
      #bpy.ops.object.mode_set(mode="EDIT")
      #bpy.ops.mesh.select_all(action="DESELECT")

      #bpy.types.SpaceView3D.use_occlude_geometry = True

      edgeIndex = 0
      for edge in self.table_orbit_data.edges:
        print("edgeIndex:%r" % edgeIndex)
        edgeIndex += 1
        self.processEdge(edge)
        #break

      return
  
    edge = self.table_orbit_data.edges[edgeIndex]
    yaw = 0
    pitch = 0

    if yawIndex is not None and pitchIndex is not None:
        
      increments = len(self.table_orbit_data.edges)
      
      radianIncrement = math.radians(360) / increments
      yaw = radianIncrement * yawIndex
      pitch = radianIncrement * pitchIndex
      
      yawIndex += 1

      if yawIndex >= increments:
        yawIndex = 0
        pitchIndex += 1
    
      if pitchIndex >= increments / 4:
        pitchIndex = 0
        yawIndex = 0
        edgeIndex += 1

    self.processEdge(edge, yaw, pitch)
  
    data = {"edgeIndex":edgeIndex, "yawIndex":yawIndex, "pitchIndex":pitchIndex}
    print("data:%r" % data)
    return data
  
  def processEdge(self, edge, yaw=None, pitch=None):
    print("processEdge(edge, yaw:%r, pitch:%r" % (yaw, pitch))
    vertexIndices = edge.vertices
    v1 = self.vertices[vertexIndices[0]].co
    #print("v1:%r" % v1)
    v2 = self.vertices[vertexIndices[1]].co
    #print("v2:%r" % v2)
    mid = (v1 + v2) / 2.0
    #print("mid:%r" % mid)

    table_camera_location = mid + self.table_orbit_matrix_world_to_translation
    self.table_camera.location = table_camera_location
    look_at(self.table_camera, self.table_orbit_location, yaw=yaw, pitch=pitch)

    object_as_camera(self.context, self.view3dAreaAndRegion, self.table_camera)

    if False:
      bpy.context.scene.objects.active = self.idris
      bpy.ops.object.mode_set(mode="EDIT")
      bpy.ops.mesh.select_mode(type="FACE")
      select_border(self.context, self.view3dAreaAndRegion)
    

def main(edgeIndex=None, yawIndex=None, pitchIndex=None):
  SelectBridge().processEdgeIndex(edgeIndex, yawIndex, pitchIndex)

def processEdgeIndexYawPitchAndIncrement(edgeIndex=None, yawIndex=None, pitchIndex=None):
  return SelectBridge().processEdgeIndex(edgeIndex, yawIndex, pitchIndex)

  
if __name__ == "__main__":
  main()

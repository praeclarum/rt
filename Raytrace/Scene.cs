using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Raytrace
{
	[Serializable]
	public class Scene
	{
		public Camera Camera { get; set; }
		
		public List<SceneObj> Objects { get; set; }
		
		public Scene ()
		{
			Camera = new Camera ();
			Objects = new List<SceneObj> ();
		}
		
		public void SaveXml (Stream s) {
			using (var w = new StreamWriter (s, Encoding.UTF8)) {
				SaveXml (w);
			}
		}
		
		public void SaveXml (TextWriter w) {
			var s = new XmlSerializer(typeof(Scene));
			s.Serialize (w, this);
		}
		
		public string SaveXml () {
			var w = new StringWriter ();
			SaveXml (w);
			return w.ToString ();
		}
		
		public static Scene Open (Stream r) {
			var s = new XmlSerializer(typeof(Scene));
			return (Scene)s.Deserialize (r);
		}
	}
	
	[Serializable]
	public class Camera {
		public Vec Position;
		public Vec Direction;
	}
	
	[Serializable]
	[XmlInclude(typeof(SphereObj))]
	public abstract class SceneObj
	{
		public SceneObj () {
		}
	}
	
	[Serializable]
	public class SphereObj : SceneObj
	{
		public Vec Center;
		public float Radius;
	}
}


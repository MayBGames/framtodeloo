using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace MayB.Tests {
  public class Sparse3DVolumeTests {
    [Test] public void BuildVertex() {
      var volume = new Sparse3DVolume();

      var vertex0 = volume.BuildVertex(0.2f, 0.4f, Vector3.one, Vector3.zero);

      Assert.AreEqual(0.3f, vertex0.x);
      Assert.AreEqual(0.3f, vertex0.y);
      Assert.AreEqual(0.3f, vertex0.z);

      var vertex1 = volume.BuildVertex(0.2f, 0.4f, -Vector3.one, Vector3.zero);

      Assert.AreEqual(-0.7f, vertex1.x);
      Assert.AreEqual(-0.7f, vertex1.y);
      Assert.AreEqual(-0.7f, vertex1.z);
    }
  }
}

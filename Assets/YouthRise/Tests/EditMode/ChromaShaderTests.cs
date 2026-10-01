using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YouthRise.Tests
{
    public class ChromaShaderTests
    {
        [Test]
        public void PortraitShaderIsIncludedAsAResource()
        {
            Shader shader = Resources.Load<Shader>("YouthRise/UIColorKey");
            Assert.That(shader, Is.Not.Null, "Player builds must include the green-screen removal shader.");
            Assert.That(shader.name, Is.EqualTo("YouthRise/UI Chroma Key"));
#if UNITY_EDITOR
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
#endif
        }
    }
}

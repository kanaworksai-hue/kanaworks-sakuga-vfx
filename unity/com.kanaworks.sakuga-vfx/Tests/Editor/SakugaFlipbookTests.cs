using KanaWorks.SakugaVFX;
using NUnit.Framework;
using UnityEngine;

public class SakugaFlipbookTests
{
    [Test]
    public void OneShotPlaysEachFrameThenReportsTheEnd()
    {
        Assert.AreEqual(-1, SakugaFlipbook.FrameAt(-0.1f, 30, 64, false));
        Assert.AreEqual(0, SakugaFlipbook.FrameAt(0f, 30, 64, false));
        Assert.AreEqual(1, SakugaFlipbook.FrameAt(1f / 30f, 30, 64, false));
        Assert.AreEqual(63, SakugaFlipbook.FrameAt(63f / 30f, 30, 64, false));
        Assert.AreEqual(64, SakugaFlipbook.FrameAt(64f / 30f, 30, 64, false));
        Assert.AreEqual(64, SakugaFlipbook.FrameAt(100f, 30, 64, false));
    }

    [Test]
    public void LoopWrapsAround()
    {
        Assert.AreEqual(0, SakugaFlipbook.FrameAt(40f / 30f, 30, 40, true));
        Assert.AreEqual(5, SakugaFlipbook.FrameAt(45f / 30f, 30, 40, true));
    }

    [Test]
    public void SidecarParsesAndConvertsThePivot()
    {
        var info = SakugaFlipbookInfo.FromJson("{\"grid\":[8,7],\"cell\":[224,400],\"frames\":56,\"bottom_inset\":0.0875," +
            "\"source\":\"water_column_01.png\",\"form\":\"column\",\"fps\":30,\"loop\":false,\"blend\":\"alpha\",\"pivot\":[0.5,0.9125]}");
        Assert.AreEqual(56, info.frames);
        Assert.IsTrue(info.GroundRooted);
        Assert.IsFalse(info.Additive);
        Assert.AreEqual(0.0875f, info.PivotUV.y, 1e-4f);
        Assert.AreEqual(new Vector2(224f / 256f, 400f / 256f), info.WorldSize);
    }

    [Test]
    public void SidecarWithoutGridIsRejected()
    {
        Assert.Throws<System.FormatException>(() => SakugaFlipbookInfo.FromJson("{\"cell\":[256,256]}"));
    }
}

public class SakugaRecipeTests
{
    const string Sidecar = "{\"grid\":[8,8],\"cell\":[256,256],\"frames\":64,\"bottom_inset\":0.1,\"source\":\"x.png\",\"id\":\"fire_impact_01\"," +
        "\"element\":\"fire\",\"form\":\"impact\",\"fps\":30,\"loop\":false,\"blend\":\"alpha\",\"pivot\":[0.5,0.5]," +
        "\"category\":\"impact\",\"style\":\"classic\",\"anchor\":\"center\",\"orientation\":\"billboard\",\"world_size_m\":2.0," +
        "\"duration_s\":2.13,\"contact_frame\":3,\"loop_range\":null,\"pairs\":{\"cast\":null,\"projectile\":\"fire_projectile_01\",\"impact\":null}," +
        "\"fx\":{\"flash\":{\"color\":\"#ffd9c0\",\"size\":1.5,\"frames\":3},\"ring\":null,\"sparks\":{\"count\":22,\"speed\":6,\"color\":\"#ff9658\"," +
        "\"gravity\":-0.15,\"drag\":2.5,\"size\":0.06,\"life\":0.75,\"direction\":\"radial\"},\"debris\":null,\"smoke\":null,\"decal\":null," +
        "\"light\":{\"color\":\"#ff7d2e\",\"intensity\":4,\"range\":4.4,\"curve\":[0,1,0.08,0.7,0.35,0.15,0.6,0]},\"trail\":null," +
        "\"shake\":{\"amplitude\":0.12,\"duration\":0.25},\"hit_stop_s\":0.03,\"sfx\":{\"cast\":null,\"loop\":null,\"impact\":\"fire_impact.wav\"}}}";

    [Test]
    public void RecipeParsesFromTheSidecarAndNullLayersAreOff()
    {
        var r = SakugaFlipbookInfo.RecipeFromJson(Sidecar);
        Assert.AreEqual("impact", r.category);
        Assert.AreEqual(3, r.contact_frame);
        Assert.AreEqual("fire_projectile_01", r.pairs.projectile);
        Assert.IsTrue(r.fx.flash.On && r.fx.sparks.On && r.fx.light.On && r.fx.shake.On);
        Assert.IsFalse(r.fx.ring.On || r.fx.debris.On || r.fx.smoke.On || r.fx.decal.On || r.fx.trail.On);
        Assert.IsFalse(r.HasLoopRange);
        Assert.AreEqual("fire_impact.wav", r.fx.sfx.impact);
    }

    [Test]
    public void LightCurveIsPiecewiseLinearAndClamped()
    {
        var l = SakugaFlipbookInfo.RecipeFromJson(Sidecar).fx.light;
        Assert.AreEqual(1f, l.Evaluate(-1f), 1e-4f);
        Assert.AreEqual(0.85f, l.Evaluate(0.04f), 1e-4f);
        Assert.AreEqual(0f, l.Evaluate(5f), 1e-4f);
        Assert.AreEqual(0.6f, l.End, 1e-4f);
    }

    [Test]
    public void EffectFrameClockHonoursTheLoopRange()
    {
        var go = new GameObject("fx");
        try
        {
            var fx = go.AddComponent<SakugaEffect>();
            fx.frames = 64; fx.fps = 30; fx.loop = false;
            fx.recipe = new SakugaRecipe { loop_range = new[] { 40, 63 } };
            Assert.AreEqual(10, fx.FrameAt(10f / 30f));
            Assert.AreEqual(63, fx.FrameAt(63f / 30f));
            Assert.AreEqual(40, fx.FrameAt(64f / 30f));                 // one pass, then 40..63 forever
            fx.recipe = new SakugaRecipe();
            Assert.AreEqual(-1, fx.FrameAt(64f / 30f));                 // a plain one-shot ends
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void SimulateIsDeterministic()
    {
        var go = new GameObject("fx");
        try
        {
            var fx = go.AddComponent<SakugaEffect>();
            fx.frames = 64; fx.fps = 30; fx.id = "test";
            fx.recipe = SakugaFlipbookInfo.RecipeFromJson(Sidecar);
            var mat = new Material(Shader.Find("KanaWorks/SakugaFlipbook"));
            fx.flashMaterial = fx.glowMaterial = fx.sparkMaterial = mat;
            fx.Build();
            fx.Simulate(0.3f);
            var a = go.GetComponentsInChildren<ParticleSystem>();
            int na = 0; foreach (var p in a) na += p.particleCount;
            var first = new ParticleSystem.Particle[64];
            var sparks = go.transform.Find("Sparks").GetComponent<ParticleSystem>();
            int n1 = sparks.GetParticles(first);
            var pos1 = first[0].position;
            fx.Simulate(0.3f);
            var again = new ParticleSystem.Particle[64];
            int n2 = sparks.GetParticles(again);
            Assert.Greater(na, 0);
            Assert.AreEqual(n1, n2);
            Assert.That(Vector3.Distance(pos1, again[0].position), Is.LessThan(1e-4f));
            Object.DestroyImmediate(mat);
        }
        finally { Object.DestroyImmediate(go); }
    }
}

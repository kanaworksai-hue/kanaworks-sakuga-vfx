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

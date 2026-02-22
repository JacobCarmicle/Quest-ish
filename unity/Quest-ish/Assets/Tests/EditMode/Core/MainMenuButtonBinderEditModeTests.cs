using System;
using NUnit.Framework;

public class MainMenuButtonBinderEditModeTests
{
    [Test]
    public void MapButtonAction_MapsCommonNames()
    {
        var t = Type.GetType("MainMenuButtonBinder, Assembly-CSharp");
        var actionEnum = Type.GetType("MenuButtonAction, Assembly-CSharp");
        Assert.IsNotNull(t);
        Assert.IsNotNull(actionEnum);

        var map = t.GetMethod("MapButtonAction");

        var newGame = map.Invoke(null, new object[] { "New Game Button" }).ToString();
        var cont = map.Invoke(null, new object[] { "Continue" }).ToString();
        var quit = map.Invoke(null, new object[] { "Exit" }).ToString();
        var none = map.Invoke(null, new object[] { "Settings" }).ToString();

        Assert.AreEqual("NewGame", newGame);
        Assert.AreEqual("Continue", cont);
        Assert.AreEqual("Quit", quit);
        Assert.AreEqual("None", none);
    }
}

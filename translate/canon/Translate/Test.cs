using System;
using Xunit;

public class TranslateTest
{
    TranslateSample sample = new TranslateSample();

    [Fact]
    public void TranslateTextTest()
    {
        string translatedText = sample.TranslateText();
        Assert.False(string.IsNullOrWhiteSpace(translatedText));
    }
}

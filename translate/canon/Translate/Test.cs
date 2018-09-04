using System;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Xunit;

public class TranslateTest
{
    TranslateSample sample = new TranslateSample();

    [Fact]
    public void TestTranslateText()
    {
        string translatedText = sample.TranslateText();
        Assert.False(string.IsNullOrWhiteSpace(translatedText));
    }

    [Fact]
    public void TestListLanguageCodes()
    {
        IList<Language> codes = sample.ListLanguageCodes();
        Assert.NotEmpty(codes);
    }

}

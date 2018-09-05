# .NET Cloud Translation API Samples

A collection of samples that demonstrate how to call the 
[Google Cloud Translation API](https://cloud.google.com/translate/) from C#.

## Build and Run

1.  **Follow the set-up instructions in [the documentation](https://cloud.google.com/dotnet/docs/setup).**

4.  Enable APIs for your project.
    [Click here](https://console.cloud.google.com/flows/enableapi?apiid=translate.googleapis.com&showconfirmation=true)
    to visit Cloud Platform Console and enable the Google Cloud Translation API.

9.  Run the test:
    ```
    $ dotnet test
    Build started, please wait...
    Build completed.

    Test run for /usr/local/google/home/rennie/gitrepos/dotnet-docs-samples/translate/canon/Translate/bin/Debug/netcoreapp2.1/Translate.dll(.NETCoreApp,Version=v2.1)
    Microsoft (R) Test Execution Command Line Tool Version 15.7.0
    Copyright (c) Microsoft Corporation.  All rights reserved.

    Starting test execution, please wait...Привет мир.
    en      Confidence: 0.882094
    af
    am
    ar
    az
    be
    bg
    bn
    bs
    ca
    ceb
    co
    cs
    cy
    da
    de
    el
    en
    eo
    es
    et
    eu
    fa
    fi
    fr
    fy
    ga
    gd
    gl
    gu
    ha
    haw
    hi
    hmn
    hr
    ht
    hu
    hy
    id
    ig
    is
    it
    iw
    ja
    jw
    ka
    kk
    km
    kn
    ko
    ku
    ky
    la
    lb
    lo
    lt
    lv
    mg
    mi
    mk
    ml
    mn
    mr
    ms
    mt
    my
    ne
    nl
    no
    ny
    pa
    pl
    ps
    pt
    ro
    ru
    sd
    si
    sk
    sl
    sm
    sn
    so
    sq
    sr
    st
    su
    sv
    sw
    ta
    te
    tg
    th
    tl
    tr
    uk
    ur
    uz
    vi
    xh
    yi
    yo
    zh
    zh-TW
    zu
    Model: NeuralMachineTranslation
    こんにちは世界。
    af      Afrikaans
    sq      Albanian
    am      Amharic
    ar      Arabic
    hy      Armenian
    az      Azerbaijani
    eu      Basque
    be      Belarusian
    bn      Bengali
    bs      Bosnian
    bg      Bulgarian
    ca      Catalan
    ceb     Cebuano
    ny      Chichewa
    zh      Chinese (Simplified)
    zh-TW   Chinese (Traditional)
    co      Corsican
    hr      Croatian
    cs      Czech
    da      Danish
    nl      Dutch
    en      English
    eo      Esperanto
    et      Estonian
    tl      Filipino
    fi      Finnish
    fr      French
    fy      Frisian
    gl      Galician
    ka      Georgian
    de      German
    el      Greek
    gu      Gujarati
    ht      Haitian Creole
    ha      Hausa
    haw     Hawaiian
    iw      Hebrew
    hi      Hindi
    hmn     Hmong
    hu      Hungarian
    is      Icelandic
    ig      Igbo
    id      Indonesian
    ga      Irish
    it      Italian
    ja      Japanese
    jw      Javanese
    kn      Kannada
    kk      Kazakh
    km      Khmer
    ko      Korean
    ku      Kurdish (Kurmanji)
    ky      Kyrgyz
    lo      Lao
    la      Latin
    lv      Latvian
    lt      Lithuanian
    lb      Luxembourgish
    mk      Macedonian
    mg      Malagasy
    ms      Malay
    ml      Malayalam
    mt      Maltese
    mi      Maori
    mr      Marathi
    mn      Mongolian
    my      Myanmar (Burmese)
    ne      Nepali
    no      Norwegian
    ps      Pashto
    fa      Persian
    pl      Polish
    pt      Portuguese
    pa      Punjabi
    ro      Romanian
    ru      Russian
    sm      Samoan
    gd      Scots Gaelic
    sr      Serbian
    st      Sesotho
    sn      Shona
    sd      Sindhi
    si      Sinhala
    sk      Slovak
    sl      Slovenian
    so      Somali
    es      Spanish
    su      Sundanese
    sw      Swahili
    sv      Swedish
    tg      Tajik
    ta      Tamil
    te      Telugu
    th      Thai
    tr      Turkish
    uk      Ukrainian
    ur      Urdu
    uz      Uzbek
    vi      Vietnamese
    cy      Welsh
    xh      Xhosa
    yi      Yiddish
    yo      Yoruba
    zu      Zulu

    Total tests: 5. Passed: 5. Failed: 0. Skipped: 0.
    Test Run Successful.
    Test execution time: 2.7462 Seconds
	```

## Contributing changes

* See [CONTRIBUTING.md](../../../CONTRIBUTING.md)

## Licensing

* See [LICENSE](../../../LICENSE)

## Testing

* See [TESTING.md](../../../TESTING.md)

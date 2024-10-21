# AstroDX

| Latest | Last Cycle (Pre-beta) |
|--------|-----------------------|
| [![Latest (Including pre-release)](https://img.shields.io/github/v/release/2394425147/astrodx?include_prereleases&sort=date)](https://github.com/2394425147/maipaddx/releases/latest) | [![Latest (Pre-beta stable)](https://img.shields.io/github/v/release/2394425147/astrodx?include_prereleases&sort=date&filter=*beta.pre*)](https://github.com/2394425147/maipaddx/releases/latest) |

AstroDX (Formerly MaipadDX) is a mobile maimai simulator intended for those who do not yet have access to a cabinet, those who want to practice, and everyone interested in maimai who otherwise could not play the arcade game.

### This game is not officially associated with maimai, maimai DX, or other SEGA products.
Please keep this in mind when producing content or sharing materials with AstroDX. It is against SEGA's policy to distribute illegal materials.


Join our Discord! [![Discord](https://dcbadge.vercel.app/api/server/6fpETgpvjZ?style=flat)](https://discord.gg/6fpETgpvjZ)  

# Open-source status

We initially intended for AstroDX to be fully open-source after it has been uploaded to official app stores, but as the game contains paid assets, we may only be able to partially open-source AstroDX.

However, if you have issues, please don't hesitate to point it out in issues and we'll try to answer them as best as we can.

This game is a clean-room implementation of a mobile maimai simulator, and has been developed without using any original arcade data.
> If you own the original copyright of this game (SEGA), or you are concerned about the clean-room implementation of this game because you have developed a maimai simulator before our first release, please contact us.
- For the simai interpreter and serializer used in AstroDX: [SimaiSharp](https://github.com/reflektone-games/SimaiSharp)
- For partial releases of source: [AstroDX core-dump](https://github.com/2394425147/maipaddx/tree/main/core-dump)

# Q&A

## Seems like my collections aren't getting installed properly in Beta

We went through a redesign for the level storage structure. You can read more here: https://github.com/2394425147/astrodx/issues/115

## Desktop version, and controller support

Currently, AstroDX hasn't released a desktop version(Windows/Linux/macOS), we also have not released any official controller support yet.

Products such as "ADX controller", "BDX", etc., some of whose names may sound similar to AstroDX, are not associated with us.

We are currently developing our controller API for the desktop version, and our API will be made public along with the desktop version. You may contact the original manufacturers of the controllers to request their support for AstroDX.

## Wait, is there a version for iOS?
Well... It's a bit finnicky. There's a major change happening regarding TestFlight (which you can read more on our Discord server [here](https://discord.com/channels/892807792996536453/1210127565986205726/1238882652040200373)).

**TL;DR: AstroDX for iOS is migrating to a new developer account, and during this period we won't free up more tester slots. However, we hope to finish this transition soon.**

In the meantime, you will still be able to join the tests as per usual.

You can join the test at [TestFlight Group A](https://testflight.apple.com/join/rACTLjPL) or [TestFlight Group B](https://testflight.apple.com/join/ocj3yptn) or [TestFlight Group C](https://testflight.apple.com/join/CuMxZE2M) or [TestFlight Group D](https://testflight.apple.com/join/T6qKfV6f).

![TestFlight](https://img.shields.io/github/downloads/2394425147/maipaddx/total?label=TestFlight)

*Each public link only can hold 10k players (they counted by Apple ID, not devices), so please DO NOT rejoin the test if you already have AstroDX installed.*

## Are there any tutorials on importing?

Detailed guides for Android and iOS are available here:
- [Installation Guide for Android](https://sht.moe/adx-android)
- [Installation Guide for iOS/iPadOS](https://rentry.org/adx_ios)

## Can I use charts transcribed from the official arcade game maimai?

We **don't recommend** doing this, as it violates SEGA's policies.

## I'm having some issues...

You can open an issue [here](https://github.com/2394425147/maipaddx/issues).

**As of May 2024, an attached English translation of the issue is mandatory.**

We welcome issues written in Chinese, Japanese and English. However, it would be strongly suggested to provide translations (even using online translator) to English when submitting them, thus other people could understand as well.

When submitting issues, please always ensure that you are running the latest released version. We also recommend reviewing existing issues to avoid duplication of questions or concerns.

Alternatively, on our [Discord server](https://discord.gg/6fpETgpvjZ), we also have a help forum dedicated for issues, an faq, as well as a suggestions channel for feedback.

## 質問がある場合

イシューは[こちら](https://github.com/2394425147/maipaddx/issues)から提出できます。

中国語、日本語、英語でのイシュー投稿を大歓迎します。ただし、他の方も理解できるように、イシューを提出する際には英文への翻訳を（オンライン翻訳を使用しても）お願いいたします。

イシューを提出する際には、最新バージョンを使用していることチェックしてください。また、重複を避けるため、既存のイシューを確認することをおすすめします。

また、AstroDXの[Discordサーバー](https://discord.gg/6fpETgpvjZ)には、質問用、FAQ、フィードバックのための提案チャンネルもご用意しています。

## 當遇到了問題的時候

可以在[這裏](https://github.com/2394425147/maipaddx/issues)提出issue。

我們歡迎使用中文、日文和英文編寫的issue。然而，在提交問題時，強烈建議提供英文翻譯（甚至使用線上翻譯），以便其他人也能理解。

提交issue時，請務必確保您正在執行的是最新發布的版本。 我們也建議審查現有issue，以避免重複或疑慮。

此外，在我們的[Discord伺服器](https://discord.gg/6fpETgpvjZ)上，還有一個專門解決問題的幫助論壇、常見問題解答以及回饋及建議頻道。

Happy playing!

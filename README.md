# Aurila

Hello, and welcome.

## Motivation

Existing Blazor frameworks suffer multiple issues, whereby they cannot be effectively be used in mobile environments, such as MAUI Blazor. They are often built with the 'web' platform in mind, and therefore they carry the limitations associated with that platform.

This means that one has to always find workarounds for various 'non-brainer' issues on mobile environments, which users have come to expect from mobile apps. One of these is the behaviour of the 'back' button on Android. In normal blazor applications, this uses a history state therefore it navigates back the history stack as it was entered, despite the fact that it may be completely nonsensical.

Consider the very natural case where an app has bottom navigation tabs. If a user navigates from tab A to B, then to C, and then back to B, the user expects that pressing the 'back' button will take them back to tab A. However, in a normal Blazor app, it will take them back to tab C, and then through the history stack until it reaches tab A.

While at that, one of the issues with Blazor is that pages lose their state when navigating away from them. This is not a problem in mobile apps, where the user expects the app to remember its state, even when navigating between different pages. For example, imagine you were scrolling through a list, then navigate to an item, when you return, you expect to be exactly where you left off, not at the top of the list.

This is even worse if the data comes from a remote source, as it may take a long time to load the data again, and the user may have to wait for it to load again, which is not a good user experience.

Another thing, infinite scroll. What app doesn't have infinite scroll these days?

Don't get me started on the mobile keyboard covering elements, especially when custom scrolling logic is involved.

These issues, alongside others, are the motivation for Aurila

## So does that mean it only works 'properly' on mobile?

Yes and no. The idea is to try make it as compatible as much as possible with the web, but the focus is on mobile. Therefore, some things may be sacrificed so as to ensure that the mobile app experience is as good as can be.

## And styling?

This is another main irk of mine with existing blazor frameworks. They are too opinionated on the design choice, and it is often not easily customisable. For example, MudBlazor is just Material 2, and Fluent Blazor is Fluent. Others like Radzen offer limited customisation.

Aurila therefore is designed to be as unopinionated as possible. It builds on top of 'Ocluse.LiquidSnow.Venus' libraray as a 'spiritual successor' of sorts.

The core library offers completely no CSS styles, and the only CSS offered is 'functional' CSS, those that are required to ensure things are rendered properly (e.g. Dropdowns). Styling comes from what I call 'appearance providers'.


An appearance provider needs to implement the `IApperanceProvider` interface, which has a single method `GetAppearance()`, which returns a `IAppearance` object. The `IAppearance` object supplies the necessary CSS classes and styles to be applied to the components.

This way, you can easily create your own appearance provider, or use one of the (soon to be) existing ones. The idea is to make it as easy as possible to create your own appearance provider, so that you can have complete control over the styling of your application.

I will be expanding further on the project and documentation on how to create your own provider.


## Finally

This project is still in its very early stages. A lot of things will change and improve over time as we at Ocluse continue to employ it in our projects and work on it. Therefore, there is not much in the way of information on how to use it, but all that will be provided in the future.
[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.SmoothCompass/master/Shared/NuGet/Icon.png "Zebble.SmoothCompass"


## Zebble.SmoothCompass

![logo]

A Zebble plugin that make you able to use compass sensor of device.


[![NuGet](https://img.shields.io/nuget/v/Zebble.SmoothCompass.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.SmoothCompass/)

> The built-in compass sensor returns data that is a bit shaky. It randomly changes in a 1-2 degree span. This causes problems if you need a smooth value for UI work (for example for Augmented Reality).
To solve this problem there is a Zebble plug-in named SmoothCompass. Instead of just the magnetic reading value of compass, it will use a combination of compass value, gyroscope and accelerometer to deliver a smooth and more natural result.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.SmoothCompass/](https://www.nuget.org/packages/Zebble.SmoothCompass/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage

In your UI module, create an instance of Smooth Compass.
In the constructor you can define the frequency of changes needed. The default is "Game" which means every 20ms, or 50 changes per second.
Handle its **Changed** event, which gives you the current compass reading (degree from North).
Dispose it in your module's Dispose event.

<br>


### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| GyroscopeChangeSensitivity           | float | x       | x   | x       |
| ToleratedError        | float | x       | x   | x       |
| TooMuchError       | float | x       | x   | x       |


<br>


### Events
| Event             | Type                                          | Android | iOS | Windows |
| :-----------      | :-----------                                  | :------ | :-- | :------ |
| Changed            | AsyncEvent   | x       | x   | x       |


<br>


### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| Create         | Task<SmoothCompass&gt;| - | x       | x   | x       |

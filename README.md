This repo was created anonymously before paper acceptance. For the latest version under maintenance, please go to https://github.com/sypei/Hand-Interfaces. Thanks!

# Hand-Interfaces

![image](https://user-images.githubusercontent.com/90154278/149648239-81732862-cc99-4865-9b34-133adf97c333.png)
http://handinterfaces.com/

The repo contains important scripts to implement retrieval pipeline, and three representative types of interfaces.

**InterfaceRetrieval.cs**: 

Interface retrieval pipeline with three example interfaces: scissors, joystick and binoculars. Developers can add other interfaces by editing the object dictionary.

**VirtualScissors.cs**: 

Interaction for Scissors (represent interfaces requiring only one emulating hand, without interacting hand)

**VirtualJoystick.cs**: 

Interaction for Joystick (represents interfaces requiring one hand as the emulating hand and the other as the interacting hand)

**VirtualBinoculars.cs**: 

Interaction for Binoculars (represent interfaces requiring two emulating hands)

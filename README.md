


# Sand Shader - in Unity URP for Mobile Phones

> Demo tested on a OnePlus 3T, runs at 55 - 60 FPS

<img src="http://theodorknab.com/wp-content/uploads/2020/09/Sand_Texture_Example.png" title="Sand Demo" alt="Sand Demo">


## Demo Scene
![](https://github.com/TheodorKnab/Sand-Shader-Unity-URP-for-Mobile/blob/master/Documentation/SandDemo.gif)

In the demo scene (Assets/Scenes/Main.unity) you can spawn stones or a rake via the buttons. 
* Left click / touch: drag object
* Long left click / touch: pick up object

## Effect Setup
This effect uses multiple shaders and materials, as multipass shaders are to my understanding currently not available in Unity SRPs.
Those multiple materials/shaders need to be controlled by a script. (Done in Assets/Scripts/Sand/DrawDepth.cs)
> How it works:

* A depth texture is created from the underside of all movable objects and stored in a render texture.

<img src="http://theodorknab.com/wp-content/uploads/2020/09/Depth_Texture.png" title="Sand Demo" alt="Sand Demo" width=45%>

* From this texture a difference texture is created, with the (stored) depth texture of the previous frame.
* This difference texture gets blured.

<img src='http://theodorknab.com/wp-content/uploads/2020/09/Difference_Texture.png' width=45%> <img src='http://theodorknab.com/wp-content/uploads/2020/09/Difference_Texture_blured.png' width = 45%>


* The blured difference texture now gets subtracted from the existing **sand depth texture**.
* The depth texture from the beginning gets added to the **sand depth texture**. 

**This texture is saved as the new sand depth texture.**

* The texture gets blured again. This texture is then used by the actual sand shader.

<img src='http://theodorknab.com/wp-content/uploads/2020/09/Depth_Texture_processed.png' width=45%> <img src='http://theodorknab.com/wp-content/uploads/2020/09/Depth_Texture_processed_blured.png' width=45%>

* The actual sand shader uses the depth map to modify the **normals** of the material. No actual displacement of the geometry happens. 
<img src="http://theodorknab.com/wp-content/uploads/2020/09/Sand_Texture_Example.png">

## License

[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](http://badges.mit-license.org)

- **[MIT license](http://opensource.org/licenses/mit-license.php)**

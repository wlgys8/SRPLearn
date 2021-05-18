# SRPLearn

基于Unity可编程渲染管线造轮子。 主要目的是为了边写边学习，不考虑平台兼容和性能优化等问题。


Unity版本: 2019.4.16

注意，项目不需要用到`com.unity.render-pipelines.universal`，可以将其从包依赖中删除。

# 目录

- [创建渲染管线，绘制Cube](https://github.com/wlgys8/SRPLearn/wiki/Hello)
- [支持平行光 - BlinnPhong光照模型](https://github.com/wlgys8/SRPLearn/wiki/DirLight)
- [平行光投影 - Shadow Mapping](https://github.com/wlgys8/SRPLearn/wiki/MainLightShadow)
- [半透明物体渲染](https://github.com/wlgys8/SRPLearn/wiki/Transparent)
- [阴影优化 - Cascade Shadow Mapping](https://github.com/wlgys8/SRPLearn/wiki/CascadeShadowMapping)
- [点光源支持 - PointLight](https://github.com/wlgys8/SRPLearn/wiki/PointLight)
- PCF软阴影
  - [理论部分 - PCF优化采样算法](https://github.com/wlgys8/SRPLearn/wiki/PCFSampleOptimize)
  - [SRP实现](https://github.com/wlgys8/SRPLearn/wiki/ShadowPCF)
- Shadow Bias
  - [理论部分 - 自适应Bias算法](https://github.com/wlgys8/SRPLearn/wiki/ShadowBias)
  - SRP实现
- 抗锯齿
  - 几种抗锯齿方式总结
  - [FXAA详细算法](https://github.com/wlgys8/SRPLearn/wiki/FXAA)
  - MSAA抗锯齿SRP实现
  - FXAA抗锯齿SRP实现
- 待补充

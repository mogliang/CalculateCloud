CalculateCloud
==============

Demo项目。搭建一个两层结构的系统，前端Web层接受用户的计算请求，做初步处理，然后递交给后端业务逻辑层受理。后端业务逻辑层受理完毕后，由前端层返回结果给用户。
前端层由WebRole实现，后端业务层由WorkerRole担任。WebRole使用Azure消息队列（Queue）向WorkerRole传递任务。WorkerRole将运算结果保存在Azure表（Table）中，由WebRole读取并显示给客户。

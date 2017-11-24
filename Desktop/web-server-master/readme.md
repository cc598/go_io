#  WEB 服务程序



*本服務器没有使用任何框架*

这是一个简单的手机归属地查询器，输入手机号码，返回其归属地以及服务提供商。

##压力测试结果
>![1][1]
>![2][2]

总请求时间：4.89s
总请求数：1000reqs
并发请求数：100reqs

吞吐率=总请求数/总请求时间=204.5(reqs/s)
用户平均请求等待时间=总请求时间/（总请求数/并发用户数）=0.489(s/reqs)
服务器平均请求等待时间=总请求时间/总请求数=0.00489(s/reqs)


***
##Curl测试结果
>![3][3]

***
##服务程序结果

###正常查询
>![4][4]
>![5][5]

###错误检测
>![6][6]
>![7][7]




[1]:https://imgsa.baidu.com/forum/w%3D580/sign=b00981b88c44ebf86d716437e9f8d736/d432ffdcd100baa11678ff004c10b912c9fc2ec0.jpg
[2]:https://imgsa.baidu.com/forum/w%3D580/sign=b8cc43909616fdfad86cc6e6848e8cea/bd0c748da9773912ac383ddaf3198618367ae27a.jpg
[3]:https://imgsa.baidu.com/forum/w%3D580/sign=7507cf2cd233c895a67e9873e1127397/8d5317385343fbf2c8321e98bb7eca8065388f7a.jpg
[4]:https://imgsa.baidu.com/forum/w%3D580/sign=ddb97a889a8fa0ec7fc764051696594a/9a06b4de9c82d15873d6e10a8b0a19d8bd3e42c0.jpg
[5]:https://imgsa.baidu.com/forum/w%3D580/sign=cc5e08beeedde711e7d243fe97eecef4/c929c8fc1e178a82a03dc71bfd03738da977e813.jpg
[6]:https://imgsa.baidu.com/forum/w%3D580/sign=dc397f94b5096b6381195e583c328733/34316009c93d70cfedb812b6f3dcd100bba12bde.jpg
[7]:https://imgsa.baidu.com/forum/w%3D580/sign=af1b16db44c2d562f208d0e5d71090f3/a0cbb04543a98226d9df939c8182b9014a90eb7a.jpg
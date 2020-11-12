---
title: Configure SSL/TLS
---
# Configure SSL/TLS

> **NOTE**: This article describes MagicOnion v3 configuration.

## SSL/TLS

As [official gRPC doc](https://grpc.io/docs/guides/auth/) notes gRPC supports SSL/TLS, and MagicOnion also support SSL/TLS. 

> gRPC has SSL/TLS integration and promotes the use of SSL/TLS to authenticate the server, and to encrypt all the data exchanged between the client and the server. Optional mechanisms are available for clients to provide certificates for mutual authentication

Let's use [samples/ChatApp/ChatApp.Server](https://github.com/Cysharp/MagicOnion/tree/master/samples/ChatApp/ChatApp.Server) for server project, and [samples/ChatApp/ChatApp.Unity](https://github.com/Cysharp/MagicOnion/tree/master/samples/ChatApp/ChatApp.Unity) for client project.

## HTTP/2 on Amazon ElasticLoadBalancer

This section explains how to setup "SSL/TLS MagicOnion on AWS" with following 5 steps.

* [Generate self-signed certificate](#generate-self-signed-certificate)
* [Server configuration](#server-configuration)
* [Kubernetes deployments](#kubernetes-deployments)
* [Enable ALPN on NLB](#enable-alpn-on-nlb)
* [Client configuration](#client-configuration)

AWS LoadBalancer supports ALPN with ALB (Application Load Balancer) and NLB (Network Load Balancer), however ALB doesn't support HTTP/2 to the backend group, you must use NLB.

**Connection flow**

To establish HTTP/2 between Client and Server with NLB, gRPC connection should follow full `HTTP/2 over TLS`.
You must establish TLS connection for both "NLB (Listener) & Client" and "NLB (TargetGroup) & Server".

```
Server <-- TLS (Self-signed) --> NLB <-- TLS (ACM) --> Client
```

* NLB (Listener) & Client: NLB Listener work with ACM. Client use Gprpc.Core.dll embedded [roots.pem](https://github.com/grpc/grpc/blob/master/etc/roots.pem) by default.
* NLB (TargetGroup) & Server: You need set TargetGroup as TLS and listen gRPC Server as TLS. You can use self-signed cert, Let's Encrypt and others. (This README use self-signed cert.)

## Generate self-signed certificate

Following command will create 3 files `server.csr`, `server.key` and `server.crt`.
gRPC/MagicOnion Server requires server.crt and server.key.

```shell
# move to your server project
$ cd samples/ChatApp/ChatApp.Server

# generate certificates
# NOTE: CN=xxxx should match domain name to magic onion server pointing domain name
$ openssl genrsa 2048 > server.key
$ openssl req -new -sha256 -key server.key -out server.csr -subj "/C=JP/ST=Tokyo/L=Tokyo/O=MagicOnion Demo/OU=Dev/CN=*.example.com"
$ openssl x509 -req -in server.csr -signkey server.key -out server.crt -days 7300 -extensions server
```

Please modify `/C=JP/ST=Tokyo/L=Tokyo/O=MagicOnion Demo/OU=Dev/CN=*.example.com` to your domain.
Make sure `CN=xxxx` should match to domain that your MagicOnion Server will recieve request from your client.

> ATTENTION: **server.key** is very sensitive file, while **server.crt** can be public. DO NOT COPY server.key to your client.

## Server configuration

> NOTE: Server will use **server.crt** and **server.key**, if you didn't copy OpenSSL generated `server.crt` and `server.key`, please back to [generate certificate](#generate-self-signed-certificate) section and copy them.

Open `samples/ChatApp/ChatApp.Server/ChatApp.Server.csproj` and add following lines before `</Project>`.

```xml
  <ItemGroup>
    <Folder Include="LinkFromUnity\" />
  </ItemGroup>

  <!-- FOR SSL/TLS SUPPORT -->
  <ItemGroup>
    <None Update="server.crt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="server.key">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```

MagicOnion.Hosting supports specifing certificate with appsettings.json.

```json
{
  "MagicOnion": {
    "ServerPorts": [
      {
        "Host": "0.0.0.0",
        "Port": 12345,
        "UseInsecureConnection": false,
        "ServerCredentials": [
          {
            "CertificatePath": "./server.crt",
            "KeyPath": "./server.key"
          }
        ]
      }
    ]
  }
}
```

Now `dotnet publish` output dll and certificates, generate your docker image and push it.

```shell
cd samples/ChatApp
docker build -t chatapp_magiconion:latest -f ChatApp.Server/Dockerfile .
docker tag chatapp_magiconion:latest your-image:latest
docker push your-image:latest
```

## Kubernetes deployments

Apply yaml to configure EKS deployments.

```shell
# replace with your ACM Arn and others.
ACM=arn:aws:acm:ap-northeast-1:123456781234:certificate/12345678-abcd-1234-efgh-1234abcd5678
DOMAIN=your-domain.com
IMAGE=your-image:latest

cat ./samples/ChatApp/k8s/nlb_acm.yaml \
    | sed -e "s|arn:aws:acm:ap-northeast-1:123456781234:certificate/12345678-abcd-1234-efgh-1234abcd5678|${ACM}|g" \
    | sed -e "s|nlb.example.com|${DOMAIN}|g" \
    | sed -e "s|cysharp/magiconion_sample_chatapp:3.0.13-chatapp|${IMAGE}|g" \
    | kubectl apply -f -
```

## Enable ALPN on NLB

Current Kubernetes Service AWS integration not support annotations for ALPN.
Configure NLB Listener's ALPN policy to `HTTP2Preferred` by following command, or via AWS Console.

```shell
$ dns=$(kubectl get svc chatapp-svc -o=jsonpath='{.status.loadBalancer.ingress[0].hostname}')
$ nlb_arn=$(aws elbv2 describe-load-balancers | jq -r ".LoadBalancers[] | select(.DNSName == \"${dns}\") | .LoadBalancerArn")
$ listeners=$(aws elbv2 describe-listeners --load-balancer-arn "${nlb_arn}" | jq -r ".Listeners[].ListenerArn")
$ for listner in "${listeners[@]}"; do aws elbv2 modify-listener --listener-arn "${listner}" --alpn-policy HTTP2Preferred; done
```

## Client configuration

> NOTE: Client will use Grpc.Core.dll embedded **roots.pem** when not specify cert file with `SslCredentials` instance.

Open `samples/ChatApp/ChatApp.Unity/Assets/ChatComponent.cs`, channel creation is defined as `ChannelCredentials.Insecure` in `InitializeClient()`.
What you need tois change this line to use `SslCredentials`.


```csharp
this.channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
```

Replace this line to following.

```csharp
this.channel = new Channel("NLB.YOUR_DOMAIN.com", 12345, new SslCredentials());
```

Play on Unity Editor and confirm Unity MagicOnion Client can connect to MagicOnion Server.

![image](https://user-images.githubusercontent.com/3856350/95848171-a632ea00-0d88-11eb-8fb4-d3b322be5fb6.png)

> NOTE: If there are any trouble establish SSL/TLS connection, Unity Client will show `disconnected server.` log.


## TLS on LocalHost

This section explains how to setup "SSL/TLS MagicOnion on localhost" with following 4 steps.

* [Generate self-signed certificate](#generate-self-signed-certificate)
* [Simulate dummy domain via hosts](#simulate-dummy-domain-via-hosts)
* [Server configuration](#server-configuration)
* [Client configuration](#client-configuration)

**Connection flow**

```
Server <-- TLS (Self-signed) --> Client
```

## Generate self-signed certificate

Certificates are required to establish SSL/TLS with Server/Client channel connection.
Let's use [OpenSSL](https://github.com/openssl/openssl) to create required certificates.

Following command will create 3 files `server.csr`, `server.key` and `server.crt`.
gRPC/MagicOnion Server requires server.crt and server.key, and Client require server.crt.

```shell
# move to your server project
$ cd MagicOnion/samples/ChatApp/ChatApp.Server

# generate certificates
# NOTE: CN=xxxx should match domain name to magic onion server pointing domain name
$ openssl genrsa 2048 > server.key
$ openssl req -new -sha256 -key server.key -out server.csr -subj "/C=JP/ST=Tokyo/L=Tokyo/O=MagicOnion Demo/OU=Dev/CN=*.example.com"
$ openssl x509 -req -in server.csr -signkey server.key -out server.crt -days 7300 -extensions server

# server will use server.crt and server.key, leave generated certificates.

# client will use server.crt, copy certificate to StreamingAssets folder.
$ mkdir ../ChatApp.Unity/Assets/StreamingAssets
$ cp server.crt ../ChatApp.Unity/Assets/StreamingAssets/server.crt
```

Please modify `/C=JP/ST=Tokyo/L=Tokyo/O=MagicOnion Demo/OU=Dev/CN=*.example.com` your domain.
Make sure `CN=xxxx` should match to domain that your MagicOnion Server will recieve request from your client.

> ATTENTION: **server.key** is very sensitive file, while **server.crt** can be public. DO NOT COPY server.key to your client.

## Simulate dummy domain via hosts

Editting `hosts` file is the simple way to redirect dummy domain request to your localhost.

Let's set your CN to you hosts, example is `dummy.example.com`. 
Open hosts file and add your entry.

```shell
# NOTE: edit hosts to test on localhost
# Windows: (use scoop to install sudo, or open elevated cmd or notepad.)
PS> sudo notepad c:\windows\system32\drivers\etc\hosts
# macos:
$ sudo vim /private/etc/hosts
# Linux:
$ sudo vim /etc/hosts
```

Entry format would be similar to this, please follow to your platform hosts rule.

```shell
127.0.0.1	dummy.example.com
```

After modifying hosts, `ping` to your dummy domain and confirm localhost is responding.

```shell
$ ping dummy.example.com

pinging to dummy.example.com [127.0.0.1] 32 bytes data:
127.0.0.1 response: bytecount =32 time <1ms TTL=128
```

## Server configuration

> NOTE: Server will use **server.crt** and **server.key**, if you didn't copy OpenSSL generated `server.crt` and `server.key`, please back to [generate certificate](#generate-certificate) section and copy them.

Open `samples/ChatApp/ChatApp.Server/ChatApp.Server.csproj` and add following lines before `</Project>`.

```xml
  <ItemGroup>
    <Folder Include="LinkFromUnity\" />
  </ItemGroup>

  <!-- FOR SSL/TLS SUPPORT -->
  <ItemGroup>
    <None Update="server.crt">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="server.key">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```


MagicOnion.Hosting supports specifing certificate with appsettings.json.

```json
{
  "MagicOnion": {
    "ServerPorts": [
      {
        "Host": "0.0.0.0",
        "Port": 12345,
        "UseInsecureConnection": false,
        "ServerCredentials": [
          {
            "CertificatePath": "./server.crt",
            "KeyPath": "./server.key"
          }
        ]
      }
    ]
  }
}
```

Debug run server on Visual Studio, any IDE or docker.

> NOTE: Set `DOTNET_ENVIRONMENT=Production` to use tls configured appsettings.json

```shell
D0729 11:08:21.767387 Grpc.Core.Internal.NativeExtension gRPC native library loaded successfully.
Application started. Press Ctrl+C to shut down.
Hosting environment: Production
```

## Client configuration

> NOTE: Client will use **server.crt**, if you didn't copy OpenSSL generated `server.crt` and `server.key`, please back to [generate certificate](#generate-certificate) section and copy it.

Open `samples/ChatApp/ChatApp.Unity/Assets/ChatComponent.cs`, channel creation is defined as `ChannelCredentials.Insecure` in `InitializeClient()`.
What you need tois change this line to use `SslCredentials`.


```csharp
this.channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);
```

Replace this line to following.

```csharp
var serverCred = new SslCredentials(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "server.crt")));
this.channel = new Channel("dummy.example.com", 5001, serverCred);
```

Play on Unity Editor and confirm Unity MagicOnion Client can connect to MagicOnion Server.

![image](https://user-images.githubusercontent.com/3856350/62017554-1be97f00-b1f2-11e9-9769-70464fe6d425.png)

> NOTE: If there are any trouble establish SSL/TLS connection, Unity Client will show `disconnected server.` log.

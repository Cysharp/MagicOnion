# MagicOnionSample

Provides a sample of a simple chat app using MagicOnion + OpenTelemetry.

This is Sample Serverside MagicOnion with OpenTelemetry implementation for Prometheus and Zipkin exporters.
You can launch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server(samples/ChatApp.Telemetry/ChatApp.Server)` project as start up and Start Debug.

> Addtional note: If you want run MagiconOnion with telemetry containers please follow to the [README](https://github.com/Cysharp/MagicOnion#try-visualization-on-localhost)

## Getting started

To run simple ChatApp.Server, 

1. Launch `ChatApp.Server` from VisualStudio.  
2. Run `ChatScene` from UnityEditor.  

If you want launch on Container, see [Container support](#container-support) section.


### ChatApp.Server

This is Sample Serverside MagicOnion with OpenTelemetry implementation for Prometheus and Zipkin exporters.
You can lanunch via Visual Studio 2019, open `MagicOnion.sln` > samples > set `ChatApp.Server.Telemetry` project as start up and Start Debug.

> Addtional note: If you want run MagiconOnion with telemetry containers please follow to the [README](https://github.com/Cysharp/MagicOnion#try-visualization-on-localhost)

## Container support

We prepare container sample with kubernetes and docker.
You may find MagicOnion on works fine on container environment.

Let's see how it work on Kubernetes, then docker.

### Docker

**run**

If you want run telemetry on Docker and run ChatApp.Server via VS, use follows.
Zipkin, Prometheus and Garafana will launch on container.

```shell
. ./docker_run_telemetry.bat
```

You can want run both MagicOnion and telemetry on container, use follows

```shell
. ./docker_run.bat
```

**dashboards**

You can access to dashboard with following urls.

* [prometheus](http://localhost:9090/)
* [zipkin](http://localhost:9411/)
* [grafana](http://localhost:3000/)

Grafana user/password is `admin/admin`.

### Kubernetes

This instruction in written under following environment.

* kubectl 1.7.6
* kubectx
* wsl

> ProTips: If you are using Windows, you can try k8s on WSL with Docker for Windows installed.

**Getting started**

Let's try run ChatApp on kubernetes cluster.

Deploy your manifests to the cluster.

```shell
kubectx docker-desktop
kubectl kustomize ./k8s/common | kubectl apply -f -
helm upgrade --install nginx-ingress --namespace chatapp stable/nginx-ingress
helm upgrade --install prometheus --namespace chatapp -f ./k8s/prometheus/values.yaml stable/prometheus
helm upgrade --install grafana --namespace chatapp -f ./k8s/grafana/values.yaml stable/grafana
```

wait until resources are launch complete.

```shell
kubectl rollout status deploy chatapp -n chatapp
kubectl rollout status deploy nginx-ingress-controller -n chatapp
kubectl rollout status deploy nginx-ingress-default-backend -n chatapp
kubectl rollout status deploy prometheus-server -n chatapp
kubectl rollout status deploy grafana -n chatapp
```

Everything is done, check kubernetes resources is up and running.

```shell
kubectl get deploy,svc,daemonset,ingress
```

Now you are ready to accept ChatApp.Unity requests.

**Access to ChatApp.Server on Kubernetes**

ChatApp.Unity can access to ChatApp.Unity on k8s with `localhost:12345`.
Just launch ChatApp.Unity and enjoy chat.

**hosts file for ingress access**

Before accesing Grafana dashboard, put `Hosts` entry to your OS, this enable your to access prometheus and grafana via ingress.

```txt
127.0.0.1 grafana.chatapp.magiconion.local
127.0.0.1 prometheus.chatapp.magiconion.local
```

**Access to the Grafana dashboard**

Let's access to your dashboard.

> Make sure you already put hosts entry.

* http://prometheus.chatapp.magiconion.local
* http://grafana.chatapp.magiconion.local

Main dashboard is Grafana, let's login with user `admin`, password will be show via below command.

```shell
kubectl get secret --namespace chatapp grafana -o jsonpath="{.data.admin-password}" | base64 --decode && echo
```

Grafana dashboard [MagicOnion Overview](https://grafana.com/grafana/dashboards/10584) will be automatically loaded into grafana, you may see your magiconion metrics.

![image](https://user-images.githubusercontent.com/3856350/83670579-5d1a9e80-a60e-11ea-9289-89a412dd5877.png)

**Clean up**

after all, clean up your k8s resources.

```shell
kubectx docker-desktop
helm uninstall nginx-ingress -n chatapp
helm uninstall prometheus -n chatapp
helm uninstall grafana -n chatapp
kubectl kustomize ./k8s/common | kubectl delete -f -
```


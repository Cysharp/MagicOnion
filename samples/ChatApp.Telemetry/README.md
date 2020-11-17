# MagicOnionSample

This is Sample to run MagicOnion with OpenTelemetry implementation.

## Getting started

Run Server and Client to confirm how it works.

1. Open console and run `docker-compose -f docker-compose.yaml -f docker-compose.telemetry.yaml up` to start server and telemetry apps.
1. Open UnityEditor and open `samples/ChatApp/ChatApp.Unity`, open `ChatScene` scene then Play Unity.
1. You can access to Telemery via Web UI Dashboards.
    * [jaeger](http://localhost:16686/)
    * [zipkin](http://localhost:9411/)
        * no data on default. you can switch with jaeger.
    * [prometheus](http://localhost:9090/)
    * [grafana](http://localhost:3000/)
        * user/password is `admin/admin`.

## Projects

There are 3 projects for this sample.

1. ChatApp.Server (samples/ChatApp.Telemetry/)
1. ChatApp.Shared (samples/ChatApp.Telemetry/)
1. ChatApp.Unity (samples/ChatApp/)

**ChatApp.Server** is Serverside MagicOnion implementation with OpenTelemetry. 

**ChatApp.Shared** is class library shared with both ChatApp.Server and ChatApp.Unity.

**ChatApp.Unity** is Unity App to connect ChatApp.Server.


## ChatApp.Server

MagicOnion is 100% capble to container.
Let's see how it work on Docker and Kubernetes.

### Docker

Docker example launch ChatApp.Server and telemetry applications on containers.

> NOTE: Make sure you have installed docker and docker-compose on your machine.

**run**

Run both ChatApp.Server and telemetry applications on container via command.

```shell
docker-compose -f docker-compose.yaml -f docker-compose.telemetry.yaml up
```

If you want run just telemetry applications on Docker, and want run ChatApp.Server via VS, use following command.

```shell
docker-compose -f docker-compose.telemetry.yaml up
```

Now ChatApp.Unity can access to ChatApp.Server running on docker.

### Kubernetes

Kubernetes example launch ChatApp.Server and telemetry applications on pods.

* kubectl 1.18
* kubectx
* wsl

> ProTips: If you are using Windows, you can use kubernetes on Docker for Windows.

**Getting started**

Let's run ChatApp on kubernetes cluster, deploy your manifests to the cluster.

```shell
cd samples/ChatApp.Telemetry
kubectx docker-desktop

helm repo add stable https://charts.helm.sh/stable
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

kubectl kustomize ./k8s/common | kubectl apply -f -
helm upgrade --install nginx-ingress --namespace chatapp stable/nginx-ingress
helm upgrade --install prometheus --namespace chatapp -f ./k8s/prometheus/values.yaml stable/prometheus
helm upgrade --install grafana --namespace chatapp -f ./k8s/grafana/values.yaml stable/grafana
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
kubens chatapp
kubectl get deploy,svc,daemonset,ingress
```

Now ChatApp.Unity can access to ChatApp.Server running on kubernetes.

### Visual Studio

You can launch ChatApp.Server on Visual Studio and run instead container.
Open `MagicOnion.Experimental.sln` and start Debug for `ChatApp.Server`.

> NOTE: please stop docker ChatApp.Server before debug on Visual Studio, due to port conflict.

## ChatApp.Unity

ChatApp.Unity is sample implementations of MagicOnion.Client running on Unity.
Once you have launched ChatApp.Server, you can use ChatApp.Unity to try how it works.

ChatApp.Unity can access to ChatApp.Unity with `localhost:5000`, launch ChatApp.Unity and enjoy chat.

## Check Telemetry

Metrics and trace are automatically collected for ChatApp.Server actions.
Telemetries are visualize for both Metrics and Trace info.

### Visualize Metrics

Prometheus collects metrics from ChatApp.Server, and Grafana visualize metrics collected via Prometheus.

**(for k8s only) hosts file for ingress access**

Before accesing Grafana dashboard, put `Hosts` entry to your OS, this enable your to access prometheus and grafana via ingress.

```txt
127.0.0.1 grafana.chatapp.magiconion.local
127.0.0.1 prometheus.chatapp.magiconion.local
127.0.0.1 jaeger.chatapp.magiconion.local
```

**access to Grafana**

Grafana visualize ChatApp.Server metrics which collected via Prometheus.
Access to Grafana dashboard.

> http://127.0.0.1:3000 (when using kubernetes, use http://grafana.chatapp.magiconion.local instead)

Login user/pass: `admin`/`admin`.

This example automatically load Grafana dashboard [MagicOnion Overview](https://grafana.com/grafana/dashboards/10584) to visualize MagicOnion metrics.

![image](https://user-images.githubusercontent.com/3856350/83670579-5d1a9e80-a60e-11ea-9289-89a412dd5877.png)

### Jaeger

Jaeger visualize tracer which sent from ChatApp.Server.
Access to Jaeger dashboard.

> http://localhost:16686/search

Select Service `chatapp.server` and click `Find Traces` to show traces.

![image](https://user-images.githubusercontent.com/3856350/99406491-46a09f00-2931-11eb-9861-86a1f1e04720.png)

> NOTE: You can switch Jaeger to Zipkin via environment vairable `UseExporter=zipkin`.

## Clean up

Clean up your resources.

* docker: `Ctrl + C` to stop docker-compose.
* kubernetes: Remove kubernetes resources.

```shell
kubectx docker-desktop
helm uninstall nginx-ingress -n chatapp
helm uninstall prometheus -n chatapp
helm uninstall grafana -n chatapp
kubectl kustomize ./k8s/common | kubectl delete -f -
```


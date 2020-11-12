---
title: MagicOnion Deployment Guide
---
# MagicOnion Deployment Guide

> **NOTE**: This article describes MagicOnion v3 configuration.

## Host in Docker
If you hosting the samples on a server, recommend to use container. Add Dockerfile like below.

```dockerfile
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS sdk
COPY . ./workspace

RUN dotnet publish ./workspace/samples/ChatApp/ChatApp.Server/ChatApp.Server.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:2.2
COPY --from=sdk /app .
ENTRYPOINT ["dotnet", "ChatApp.Server.dll"]

# Expose ports.
EXPOSE 12345
```

And docker build, send to any container registory.

Here is the sample of deploy AWS [ECR](https://us-east-2.console.aws.amazon.com/ecr/) and [ECS](https://us-east-2.console.aws.amazon.com/ecs) by CircleCI.
```yml
version: 2.1
orbs:
# see: https://circleci.com/orbs/registry/orb/circleci/aws-ecr
# use Environment Variables : AWS_ECR_ACCOUNT_URL
#                             AWS_ACCESS_KEY_ID	
#                             AWS_SECRET_ACCESS_KEY
#                             AWS_REGION  
  aws-ecr: circleci/aws-ecr@4.0.1
# see: https://circleci.com/orbs/registry/orb/circleci/aws-ecs
# use Environment Variables : AWS_ACCESS_KEY_ID	
#                             AWS_SECRET_ACCESS_KEY
#                             AWS_REGION
  aws-ecs: circleci/aws-ecs@0.0.7
workflows:
  build-push:
    jobs:
      - aws-ecr/build_and_push_image:
          repo: sample-magiconion
      - aws-ecs/deploy-service-update:
          requires:
            - aws-ecr/build_and_push_image
          family: 'sample-magiconion-service'
          cluster-name: 'sample-magiconion-cluster'
          container-image-name-updates: 'container=sample-magiconion-service,tag=latest'
          
```

Here is the sample of deploy [Google Cloud Platform(GCP)](https://console.cloud.google.com/) by CircleCI.
```yml
version: 2.1
orbs:
  # see: https://circleci.com/orbs/registry/orb/circleci/gcp-gcr
  # use Environment Variables : GCLOUD_SERVICE_KEY
  #                             GOOGLE_PROJECT_ID
  #                             GOOGLE_COMPUTE_ZONE
    gcp-gcr: circleci/gcp-gcr@0.6.0
workflows:
    build_and_push_image:
        jobs:
            - gcp-gcr/build-and-push-image:
                image: sample-magiconion
                registry-url: asia.gcr.io # other: gcr.io, eu.gcr.io, us.gcr.io
```

Depending on the registration information of each environment and platform, fine tuning may be necessary, so please refer to the platform documentation and customize your own.

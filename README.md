![](res/Title.jpg)
# Building API Gateway with load balancer and service discovery

In my [previous demo](https://github.com/matjazbravc/Microservice.Architecture.Demo) I showed how to implement microservice architecture using ASP.NET Core. This demo expands the microservice architecture with gateway load balancer ([Ocelot](https://github.com/ThreeMammals/Ocelot)) and service discovery ([Eureka](https://spring.io/projects/spring-cloud-netflix)).

## Load balancing
Load balancing improves the distribution of workloads across multiple computing resources. Load balancing aims to optimize resource use, maximize throughput, minimize response time, and avoid overload of any single resource. [Ocelot](https://github.com/ThreeMammals/Ocelot) can load balance across available downstream services for each Route. This means you can scale your downstream services and [Ocelot](https://github.com/ThreeMammals/Ocelot) can use them effectively. The type of load balancer available are:

* **LeastConnection** - tracks which services are dealing with requests and sends new requests to service with least existing requests. The algorythm state is not distributed across a cluster of Ocelot’s.

* **RoundRobin** - loops through available services and sends requests. The algorythm state is not distributed across a cluster of Ocelot’s.

* **NoLoadBalancer** - takes the first available service from config or service discovery.

* **CookieStickySessions** - uses a cookie to stick all requests to a specific server. More info below.

We will use **RoundRobin** load balancer. Why? **RoundRobin** load balancing is one of the simplest methods for distributing client requests across a group of servers. Going down the list of servers in the group, the **RoundRobin** load balancer forwards a client request to each server in turn. When it reaches the end of the list, the load balancer loops back and goes down the list again (sends the next request to the first listed server, the one after that to the second server, and so on).

## Service discovery
Service discovery is the automatic detection of services on a computer network. A service registry is useful because it enables client-side load-balancing and decouples service providers from consumers without the need for DNS.
Ocelot allows you to specify a service discovery provider and will use this to find the host and port for the downstream service [Ocelot](https://github.com/ThreeMammals/Ocelot) is forwarding a request to. This is only supported in the **GlobalConfiguration** section which means the same service discovery provider will be used for all Routes you specify a ServiceName for at Route level.
There are some types of service discovery we can use in [Ocelot](https://github.com/ThreeMammals/Ocelot), such as [Consul](https://www.consul.io/docs/intro), [Eureka](https://spring.io/projects/spring-cloud-netflix), and so on. In this demo we will use and set up a Netflix Eureka service registry.
Ocelot uses [Steeltoe](https://steeltoe.io/) to communicate with [Eureka](https://spring.io/projects/spring-cloud-netflix), which is an open source project that enables .NET developers to implement industry standard best practices when building resilient microservices.

## Configuration
The following configuration shows how to set up multiple downstream services for a **Routes** using **ocelot.json** and use the **RoundRobin** load balancer and **Eureka** service discovery.
```json
{
  "Routes": [
    {
      "ServiceName": "SERVICE.OPENAPI",
      "DownstreamPathTemplate": "/{everything}",
      "DownstreamScheme": "http",
      "UpstreamPathTemplate": "/{everything}",
      "UpstreamHttpMethod": [ "GET" ],
      "UseServiceDiscovery": true,
      "RouteIsCaseSensitive": false,
      "LoadBalancerOptions": {
        "Type": "RoundRobin"
      },
      "QoSOptions": {
        "ExceptionsAllowedBeforeBreaking": 3,
        "DurationOfBreak": 5000,
        "TimeoutValue": 2000
      }
    }
  ],
  "GlobalConfiguration": {
    "RequestIdKey": "OcRequestId",
    "DownstreamScheme": "http",
    "UseServiceDiscovery": true
  }
}
```

## Architecture
![](res/Architecture.jpg)

#### Services:
* **Services.Gateway**: Gateway to all APIs
* **ServiceA.OpenApi**: Simple OpenAPI REST service
* **ServiceB.OpenApi**: Simple OpenAPI REST service
* **ServiceC.OpenApi**: Simple OpenAPI REST service

First, each service instance registers itself to the service discovery service by providing its name and address.

<u>**NOTE: ALL SERVICES MUST HAVE THE SAME REGISTRY NAME "SERVICE.OPENAPI**"</u>

After this step client is able to get information about this service by querying the service discovery.

**ServiceA.OpenApi** appsettings.json file:
```json
{
  "Spring": {
    "Application": {
      "Name": "SERVICE.OPENAPI"
    }
  },
  // Eureka settings
  // https://docs.steeltoe.io/api/v4/discovery/netflix-eureka.html
  "Eureka": {
    "Client": {
      "ServiceUrl": "http://localhost:8761/eureka/",
      "ShouldFetchRegistry": true,
      "ShouldRegisterWithEureka": true,
      "Health": {
        "CheckEnabled": true
      },
      "RegistryFetchIntervalSeconds": "15",
      "ValidateCertificates": false,
      "EurekaServer": {
        "ConnectTimeoutSeconds": 30
      }
    },
    "Instance": {
      "NonSecurePort": 9100,
      "HostName": "localhost",
      "InstanceId": "ServiceA.OpenApi,Port:9100",
      "HealthCheckUrlPath": "/health",
      "StatusPageUrlPath": "/api/values/status",
      "LeaseRenewalIntervalInSeconds": "5"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
**ServiceB.OpenApi** appsettings.json file:
```json
{
  "Spring": {
    "Application": {
      "Name": "SERVICE.OPENAPI"
    }
  },
  // Eureka settings
  // https://docs.steeltoe.io/api/v4/discovery/netflix-eureka.html
  "Eureka": {
    "Client": {
      "ServiceUrl": "http://localhost:8761/eureka/",
      "ValidateCertificates": false,
      "ShouldRegisterWithEureka": true,
      "Health": {
        "CheckEnabled": true
      },
      "RegistryFetchIntervalSeconds": "15",
      "EurekaServer": {
        "ConnectTimeoutSeconds": 30
      }
    },
    "Instance": {
      "NonSecurePort": 9200,
      "HostName": "localhost",
      "InstanceId": "ServiceB.OpenApi,Port:9200",
      "HealthCheckUrlPath": "/health",
      "StatusPageUrlPath": "/api/values/status",
      "LeaseRenewalIntervalInSeconds": "5"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
**ServiceC.OpenApi** appsettings.json file:
```json
{
  "Spring": {
    "Application": {
      "Name": "SERVICE.OPENAPI"
    }
  },
  // Eureka settings
  // https://docs.steeltoe.io/api/v4/discovery/netflix-eureka.html
  "Eureka": {
    "Client": {
      "ServiceUrl": "http://localhost:8761/eureka/",
      "ValidateCertificates": false,
      "ShouldRegisterWithEureka": true,
      "Health": {
        "CheckEnabled": true
      },
      "RegistryFetchIntervalSeconds": "15",
      "EurekaServer": {
        "ConnectTimeoutSeconds": 30
      }
    },
    "Instance": {
      "NonSecurePort": 9300,
      "HostName": "localhost",
      "InstanceId": "ServiceC.OpenApi,Port:9300",
      "HealthCheckUrlPath": "/health",
      "StatusPageUrlPath": "/api/values/status",
      "LeaseRenewalIntervalInSeconds": "5"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
# Steps To Run the Solution locally
You can easily test and debug solution locally. You have to ...

1. Set all project in solution to start project launch settings like this
![](res/RunLocal1.jpg)

2. Select multiple startup projects:
![](res/RunLocal2.jpg)

3. Spin up Eureka in Docker with:  
**docker run -p 8761:8761 steeltoeoss/eurekaserver:1.4.4**  
and wait a minute to start up ...

4. Run multiple projects from Visual studio

# Running the Tests
Each project has its own test project in the **tests** folder:

* **ServiceA.OpenApi.Tests**, **ServiceB.OpenApi.Tests**, **ServiceC.OpenApi.Tests** - endpoints of each service (`/`, `/api/values`, `/api/values/status`, `/health` and the `badcode` error)
* **Services.Gateway.Tests** - the gateway's own endpoints, that `/api/values` is routed by Ocelot through the Eureka provider, and the Ocelot route configuration (RoundRobin, QoS)

The tests host each app in memory with **xUnit v3** and `WebApplicationFactory`, with the Eureka client switched off, so neither Eureka nor Docker is needed. Run them all from the solution folder with:

```
dotnet test --solution Ocelot.Gateway.Eureka.ServiceDiscovery.Demo.sln
```

The tests use **Microsoft.Testing.Platform**, which is selected for `dotnet test` in **global.json**. They also run from the Test Explorer in Visual Studio.

# Running in Docker

## Docker-Compose Files
**Docker-compose.yml** file with setup for all the containers looks like this:
```yml
services:
    eureka-server:
        container_name: eureka-server
        image: steeltoeoss/eurekaserver:1.4.4
        restart: on-failure
        hostname: eureka-server
        healthcheck:
            test: ["CMD", "curl", "-fs", "-o", "/dev/null", "http://localhost:8761/eureka/apps"]
            interval: 3s
            timeout: 3s
            retries: 40
        networks:
            - common_network

    servicea.openapi:
        container_name: servicea.openapi
        image: servicea.openapi:latest
        restart: on-failure
        hostname: servicea.openapi
        build:
            context: .
            dockerfile: src/ServiceA.OpenApi/Dockerfile
        networks:
            - common_network

    serviceb.openapi:
        container_name: serviceb.openapi
        image: serviceb.openapi:latest
        restart: on-failure
        hostname: serviceb.openapi
        build:
            context: .
            dockerfile: src/ServiceB.OpenApi/Dockerfile
        networks:
            - common_network

    servicec.openapi:
        container_name: servicec.openapi
        image: servicec.openapi:latest
        restart: on-failure
        hostname: servicec.openapi
        build:
            context: .
            dockerfile: src/ServiceC.OpenApi/Dockerfile
        networks:
            - common_network

    services.gateway:
        container_name: services.gateway
        image: services.gateway:latest
        restart: on-failure
        hostname: services.gateway
        build:
            context: .
            dockerfile: src/Services.Gateway/Dockerfile
        networks:
            - common_network

networks:
    common_network:
        driver: bridge
```
and **Docker-compose.override.yml** file:
```yml
services:
    eureka-server:
        environment:
            - EUREKA_SERVER_ENABLE_SELF_PRESERVATION=false
        ports:
            - 8761:8761

    servicea.openapi:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=http://+:9100
            - Eureka__Client__ServiceUrl=http://eureka-server:8761/eureka/
        ports:
            - 9100:9100
        depends_on:
            eureka-server:
                condition: service_healthy

    serviceb.openapi:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=http://+:9200
            - Eureka__Client__ServiceUrl=http://eureka-server:8761/eureka/
        ports:
            - 9200:9200
        depends_on:
            eureka-server:
                condition: service_healthy

    servicec.openapi:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - ASPNETCORE_URLS=http://+:9300
            - Eureka__Client__ServiceUrl=http://eureka-server:8761/eureka/
        ports:
            - 9300:9300
        depends_on:
            eureka-server:
                condition: service_healthy

    services.gateway:
        environment:
            - ASPNETCORE_ENVIRONMENT=Docker
            - Eureka__Client__ServiceUrl=http://eureka-server:8761/eureka/
        ports:
            - 9500:8080
        depends_on:
            eureka-server:
                condition: service_healthy
            servicea.openapi:
                condition: service_started
            serviceb.openapi:
                condition: service_started
            servicec.openapi:
                condition: service_started
```
# Setup the Containers
To execute compose file, open Powershell, and navigate to the compose file in the root folder. Then execute the following command: **docker-compose up -d --build --remove-orphans**. The -d parameter executes the command detached. This means that the containers run in the background and don’t block your Powershell window. To check all running Containers use **docker ps**.

![](res/Docker.jpg)

## Check it out
Let’s make a first call through **[API Gateway](http://localhost:9500/api/values)**. As we can see, we got response from **Service-A**
![](res/Gateway1.jpg)
With another request we got response from **Service-C**
![](res/Gateway2.jpg)
And again, we make another request and we got response from **Service-B**
![](res/Gateway3.jpg)
And so on...

## Conclusion
[Ocelot](https://github.com/ThreeMammals/Ocelot) successfuly comunicates with [Eureka](https://spring.io/projects/spring-cloud-netflix) service registry and retrives service registrations, load balancer loops through available services and sends requests. It works! Enjoy!

## Prerequisites
- [Visual Studio](https://www.visualstudio.com/vs/community) 2026 or greater
- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/resources/what-container)

## Tags & Technologies
- [.NET 10](https://github.com/dotnet/core/blob/main/release-notes/10.0)
- [Ocelot](https://github.com/ThreeMammals/Ocelot)  
- [Eureka](https://spring.io/projects/spring-cloud-netflix)

## Licence
Licenced under [MIT](http://opensource.org/licenses/mit-license.php).
Contact me on [LinkedIn](https://si.linkedin.com/in/matjazbravc).

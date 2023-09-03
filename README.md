# Optimized URL Shortener Service 🚀

A URL shortener that stands out by pre-generating shortened URLs. This approach optimizes response times and minimizes database interactions during a shorten request, ensuring faster and more efficient URL shortening experiences.

## Table of Contents

- [Features](#features)
- [Upcoming](#upcoming)
- [Tech Stack](#tech-stack)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Contributing](#contributing)
- [License](#license)

## Features 🌟

- [x] **IHostedService**: Generates shortened URLs in the background. Pre-generating URLs ensures that the system can immediately respond to shortening requests without the need for on-the-fly generation. This not only speeds up the response time but also reduces the load on the database, as there's no need to constantly check for URL uniqueness during high request volumes.
- [x] **Polly**: Provides retry mechanisms for URL fetching.
- [x] **Redirection**: Redirects users using the shortened URL.

## Upcoming 🔮
- [ ] **RateLimiting**: Implement rate limiting to safeguard the API from excessive requests and potential abuse.
- [ ] **reCaptcha**: Add bot detection mechanisms during URL submission to prevent automated spam.
- [ ] **Serilog & Seq**: Integrate structured logging for better traceability and debugging capabilities.
- [ ] **Prometheus & Grafana**: Set up system metrics monitoring and visualization to keep an eye on performance and potential bottlenecks.
- [ ] **Analytics**: Collect and analyze metrics on how often each shortened URL is accessed, offering insights into user behavior.
- [ ] **Custom Aliases**: Allow users to specify custom short names for their URLs, e.g., ``short.url/myname`` instead of a random string.
- [ ] **Expiration**: Introduce the ability to set expiration dates for shortened URLs, if they are not accessed for a while.
- [ ] **QR Code Generation**: Offer a QR code representation for each shortened URL, facilitating sharing in various formats.
- [ ] **Bulk Shortening**: Develop an endpoint that allows users to submit and shorten multiple URLs in a single request.

## Tech Stack 🛠️

- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Dapper](https://dapper-tutorial.net/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [Polly](https://github.com/App-vNext/Polly)

## How It Works

1. **Background Service**: Generates shortened URLs and sends them to RabbitMQ.
2. **Shortening Endpoint**: Fetches URLs with retry logic using Polly.
3. **Redirection Endpoint**: Retrieves and redirects to the original URL.

## Getting Started 🚀

### Prerequisites

- .NET SDK
- Docker
- PostgreSQL

Create a database called ``UrlShortenerDb`` then run the sql scripts located ``Database/Migrations``.

### Installation

```bash
docker-compose up -d
```



```bash
dotnet run
```

### Contributing 🤝
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

### License 📝
Distributed under the Apache-2.0 License. See `LICENSE` for more information.
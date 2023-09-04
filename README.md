# Optimized URL Shortener Service 🚀

A URL shortener that leverages Redis for caching and ensuring unique URL entries, alongside RabbitMQ for managing a queue of pre-generated shortened URLs. This setup not only speeds up response times by facilitating immediate URL retrieval from RabbitMQ but also minimizes database interactions and storage usage by preventing duplicate entries for the same URL, resulting in a more efficient system.

## Table of Contents

- [Features](#features)
- [Upcoming](#upcoming)
- [Tech Stack](#tech-stack)
- [How It Works](#how-it-works)
- [Getting Started](#getting-started)
- [Contributing](#contributing)
- [License](#license)

## Features 🌟

- [x] **MessageQueue**: Leverages RabbitMQ to store pre-generated shortened URLs in a queue, facilitating rapid retrieval and enhancing scalability by minimizing response times during peak traffic periods.
- [x] **IHostedService**: Operates in the background to generate shortened URLs, allowing the system to instantly respond to shortening requests without real-time generation. This approach not only accelerates response times but also alleviates database strain by eliminating the need for continuous URL uniqueness checks during high traffic periods.
- [x] **Caching**: Employed to swiftly retrieve previously shortened URLs and ensure record uniqueness, reducing the necessity for frequent database interactions and optimizing system performance.
- [x] **Polly**: Implements retry mechanisms for reliable URL fetching.
- [x] **Redirection**: Facilitates user redirection using the shortened URL.

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
- [Redis](https://redis.io/)
- [Polly](https://github.com/App-vNext/Polly)

## How It Works

1. **Background Service**: Continuously generates shortened URLs in the background and dispatches them to a RabbitMQ queue, ready for immediate use.
2. **Shortening Endpoint**:
    1. Initially checks the Redis cache to determine if the URL has been previously shortened, ensuring unique records without frequent database interactions.
    2. If not found, consults the database to check if the URL has been previously shortened (it might be in the database but expired from the cache).
    3. If still not found, retrieves a pre-generated shortened URL from the RabbitMQ queue, utilizing a retry logic implemented with Polly.
    4. Saves the newly generated shortened URL in both the Redis cache, for rapid future retrieval, and the database, ensuring data persistence.

3. **Redirection Endpoint**: Retrieves the original URL associated with the shortened URL directly from the database, given that the IDistributedCache lacks an implementation for key (originalUrl) retrieval, a functionality that is generally uncommon, and subsequently redirects the user to the original URL.

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
# Optimized URL Shortener Service

This is a fun and simple implementation of a URL shortener service with a different approach. Instead of generating shortened URLs on-the-fly, this service pre-generates a pool of unique shortened URLs in the background. This design aims to provide immediate availability and reduce database checks during the URL shortening process.

## Highlights

- **Background URL Generation**: The service continuously generates unique shortened URLs in the background, ensuring a pre-generated URL is always ready for use.
  
- **Deferred Database Insertion**: The service provides an immediate response with a shortened URL. The mapping between the original and shortened URL is stored in the database asynchronously.
  
- **Concurrency-Proof**: Uses a `ConcurrentQueue` to handle multiple simultaneous requests without race conditions or potential duplication.
  
- **Database Integration**: Integrates with a PostgreSQL database to store the mapping between the original URLs and their shortened counterparts.

## Cutting-Edge Tech Stack

- **Latest .NET**: This project is built using the latest .NET 8.
  
- **Best Practices**: I am trying to follow the latest best practices as recommended in the Microsoft docs for .NET 8 and from the my favourite .NET content creators.

## How It Works

1. **Background Service**: Continuously generates unique shortened URLs, checks the database for uniqueness, and populates a `ConcurrentQueue`.
  
2. **Shortening Endpoint**: On request, dequeues a pre-generated shortened URL and returns it. The mapping is stored in the database asynchronously.
  
3. **Redirection Endpoint**: Looks up the original URL in the database and redirects the user to it.

## Setup and Configuration

TODO

## Note

This project was created for fun to showcase a unique approach to URL shortening. It's a simple implementation and not intended for production use but is built using the latest technologies and best practices.

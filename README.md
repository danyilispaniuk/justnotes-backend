# MongoDB + .NET Backend (Codespaces-friendly)

A hands-on starter to learn MongoDB while building a small .NET backend. This repository focuses on practical examples: 
* introducing common MongoDB concepts
* performing CRUD operations.
* connecting a .NET (ASP.NET Core) API to MongoDB

## Goals
- Learn the basics of MongoDB (documents, collections, CRUD, indexes, aggregation).
- Build a simple .NET backend that uses the official MongoDB .NET driver and Minimal APIs.
- Be able to run everything locally.

## Prerequisites
- This is a Template repository, simply click on **Use this template** to get a copy of this repository on your GitHub account.

## Testing with mongosh 
- Use `mongosh` to inspect the database:
  - `mongosh "mongodb://localhost:27017"` or simply `mongosh`
  - `show dbs;`
  - `use mydb;`
  - `db.items.find().pretty()`

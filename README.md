```podman pull postgres:alpine```

```podman run --name my-postgres -e POSTGRES_PASSWORD=mysecretpassword -d -p 55432:5432 -v postgres_data:/var/lib/postgresql/data postgres:alpine```

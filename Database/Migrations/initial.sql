CREATE TABLE "Urls" (
	"Id" SERIAL PRIMARY KEY NOT NULL,
	"OriginalUrl" VARCHAR(4096) NOT NULL,
	"ShortenedUrl" VARCHAR(4096) UNIQUE
);
CREATE SCHEMA IF NOT EXISTS app AUTHORIZATION "WalkthroughUser";

CREATE SEQUENCE IF NOT EXISTS app."metrics_seq";

CREATE TABLE IF NOT EXISTS app."Metrics"
(
    "MetricId" integer NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "WorkerCount" integer NOT NULL,
    "WorkLoadSize" integer NOT NULL,
    "ConsumerCount" integer NOT NULL,
    "ConsumerThroughput" integer NOT NULL,
    "QueueSize" integer NOT NULL,
    "PublishRate" numeric(8,2) NOT NULL,
    "ConsumeRate" numeric(8,2) NOT NULL,
    CONSTRAINT "PK_Metrics" PRIMARY KEY ("MetricId")
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS app."Metrics" OWNER to "WalkthroughUser";

ALTER TABLE app."Metrics" ALTER COLUMN "MetricId" SET DEFAULT nextval('app.metrics_seq');

ALTER SEQUENCE app."metrics_seq" OWNED BY app."Metrics"."MetricId";



---
CREATE SEQUENCE IF NOT EXISTS app."messages_seq";

CREATE TABLE IF NOT EXISTS app."Messages"
(
    "MessageId" integer NOT NULL,
    "Stored" timestamp with time zone,
    "Processed" timestamp with time zone,
    "TimeSpent" time without time zone NULL,
    "Num" integer,
    CONSTRAINT "PK_Messages" PRIMARY KEY ("MessageId")
) 
TABLESPACE pg_default;

ALTER TABLE IF EXISTS app."Messages" OWNER to "WalkthroughUser";

ALTER TABLE app."Messages" ALTER COLUMN "MessageId" SET DEFAULT nextval('app.messages_seq');

ALTER SEQUENCE app."messages_seq" OWNED BY app."Messages"."MessageId";
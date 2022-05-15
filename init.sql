CREATE TABLE todos (
    "id" UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    "text" TEXT NOT NULL,
    "isChecked" bool DEFAULT false NOT NULL,
    "createdAt" TIMESTAMP DEFAULT now(),
    "updatedAt" TIMESTAMP DEFAULT now()
);
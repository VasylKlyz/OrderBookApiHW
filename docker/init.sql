IF NOT EXISTS (
    SELECT [name]
        FROM sys.databases
        WHERE [name] = N'OrderBookApiHW'
)
CREATE DATABASE OrderBookApiHW
GO
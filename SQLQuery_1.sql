CREATE TABLE Owner
(
    Id          INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Name        VARCHAR(20),
    PhoneNumber VARCHAR(11),
    Email       VARCHAR(30),
);

DECLARE @counter smallint
SET @counter = 0
WHILE @counter < 15000
   BEGIN
   INSERT INTO [Owner]
        Values ('user-'+ CAST(@counter as varchar), '09011111111', 'user-'+ CAST(@counter as varchar)+ '@test.mail')
      SET @counter = @counter + 1
   END
GO

-- SELECT * FROM [Owner]
-- TRUNCATE TABLE [Owner]

CREATE TABLE Vehicle
(
   Id          INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
   Vin         VARCHAR(20) NOT NULL,
   Dealer      VARCHAR(20) NOT NULL,
   OwnerId     INT NOT NULL REFERENCES Owner (Id),
);

DECLARE @counter smallint
SET @counter = 0
WHILE @counter < 20000
   BEGIN
   INSERT INTO [Vehicle]
        Values ('vin-'+ CAST(@counter as varchar), 'Dealer-'+ CAST(@counter as varchar), RAND() * 1000)
      SET @counter = @counter + 1
   END
GO

-- SELECT * FROM [Vehicle]
-- TRUNCATE TABLE [vehicle]
-- DROP TABLE [Vehicle]


CREATE TABLE UkkariInfoMaster
(
   Id          INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
   Level       VARCHAR(8),
   Message     VARCHAR(100)
);

INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'Leave a window open')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'Leave a door open')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'Leave a refueling port open')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'with the bag left')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'with the key left')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'with the children left')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'Leave me alone')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'lost right arm')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'lost left arm')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'lost right leg')
INSERT INTO [UkkariInfoMaster] VALUES ('Warn', 'lost left leg')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'sheet belt broken')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'front door broken')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'right door broken')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'left door broken')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'car leave broken')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'a engine broken')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'engine oil used up')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'A Brake is stolen')
INSERT INTO [UkkariInfoMaster] VALUES ('Critical', 'A Handle is stolen')

-- SELECT * FROM [UkkariInfoMaster]

-- TRUNCATE TABLE [UkkariInfoMaster]


SELECT * FROM [Vehicle]
  INNER JOIN [Owner]
     ON [Vehicle].OwnerId = [Owner].Id




CREATE TABLE Location
(   
    DateTime VARCHAR(30),
    TransactionId VARCHAR(50),
    VehicleId VARCHAR(20),
    Address VARCHAR(20),
    City VARCHAR(20)
);



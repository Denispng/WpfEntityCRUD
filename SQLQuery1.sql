CREATE TABLE Aisles (
    AisleID INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(50),
    LayerCount INT,
    CellCount INT
);

CREATE TABLE Cells (
    CellID INT PRIMARY KEY IDENTITY,
    AisleID INT FOREIGN KEY REFERENCES Aisles(AisleID),
    XCoordinate INT,
    YCoordinate INT,
    ZCoordinate INT,
    Id INT NULL FOREIGN KEY REFERENCES members(Id)
);
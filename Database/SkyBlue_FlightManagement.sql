-- ==================== TABLES ====================

CREATE TABLE [dbo].[Countries](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE TABLE [dbo].[Roles](
	[ID] [int] NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE TABLE [dbo].[Offices](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CountryID] [int] NOT NULL,
	[Title] [nvarchar](50) NOT NULL,
	[Phone] [nvarchar](50) NOT NULL,
	[Contact] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Office] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_Office_Country] FOREIGN KEY ([CountryID]) REFERENCES [Countries]([ID])
)
GO

CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[Email] [nvarchar](150) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[OfficeID] [int] NULL,
	[Birthdate] [date] NULL,
	[Active] [bit] NULL DEFAULT 1,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [UQ_User_Email] UNIQUE NONCLUSTERED ([Email] ASC),
 CONSTRAINT [FK_User_Role] FOREIGN KEY ([RoleID]) REFERENCES [Roles]([ID]),
 CONSTRAINT [FK_User_Office] FOREIGN KEY ([OfficeID]) REFERENCES [Offices]([ID])
)
GO

CREATE TABLE [dbo].[Airports](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CountryID] [int] NOT NULL,
	[IATACode] [varchar](3) NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_AirPort] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_Airport_Country] FOREIGN KEY ([CountryID]) REFERENCES [Countries]([ID])
)
GO

CREATE TABLE [dbo].[Routes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DepartureAirportID] [int] NOT NULL,
	[ArrivalAirportID] [int] NOT NULL,
	[Distance] [int] NOT NULL,
	[FlightTime] [int] NOT NULL,
 CONSTRAINT [PK_Routes] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_Route_DepAirport] FOREIGN KEY ([DepartureAirportID]) REFERENCES [Airports]([ID]),
 CONSTRAINT [FK_Route_ArrAirport] FOREIGN KEY ([ArrivalAirportID]) REFERENCES [Airports]([ID])
)
GO

CREATE TABLE [dbo].[Aircrafts](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[MakeModel] [nvarchar](50) NULL,
	[TotalSeats] [int] NOT NULL,
	[FirstClassSeats] [int] NOT NULL DEFAULT 0,
	[EconomySeats] [int] NOT NULL,
	[BusinessSeats] [int] NOT NULL,
 CONSTRAINT [PK_AirPlan] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE TABLE [dbo].[Schedules](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Date] [date] NOT NULL,
	[Time] [time](5) NOT NULL,
	[AircraftID] [int] NOT NULL,
	[RouteID] [int] NOT NULL,
	[EconomyPrice] [money] NOT NULL,
	[Confirmed] [bit] NOT NULL DEFAULT 1,
	[FlightNumber] [nvarchar](10) NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_Schedule_Aircraft] FOREIGN KEY ([AircraftID]) REFERENCES [Aircrafts]([ID]),
 CONSTRAINT [FK_Schedule_Route] FOREIGN KEY ([RouteID]) REFERENCES [Routes]([ID])
)
GO

CREATE TABLE [dbo].[CabinTypes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[PriceMultiplier] [float] NULL,
 CONSTRAINT [PK_ClassType] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE TABLE [dbo].[Amenities](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Service] [nvarchar](50) NOT NULL,
	[Price] [money] NOT NULL,
 CONSTRAINT [PK_Amenity] PRIMARY KEY CLUSTERED ([ID] ASC)
)
GO

CREATE TABLE [dbo].[AmenitiesCabinType](
	[CabinTypeID] [int] NOT NULL,
	[AmenityID] [int] NOT NULL,
 CONSTRAINT [PK_AmenitiesCabinType] PRIMARY KEY CLUSTERED ([CabinTypeID] ASC, [AmenityID] ASC),
 CONSTRAINT [FK_ACT_Cabin] FOREIGN KEY ([CabinTypeID]) REFERENCES [CabinTypes]([ID]),
 CONSTRAINT [FK_ACT_Amenity] FOREIGN KEY ([AmenityID]) REFERENCES [Amenities]([ID])
)
GO

CREATE TABLE [dbo].[Tickets](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[ScheduleID] [int] NOT NULL,
	[CabinTypeID] [int] NOT NULL,
	[Firstname] [nvarchar](50) NOT NULL,
	[Lastname] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](150) NULL,
	[Phone] [nvarchar](14) NOT NULL,
	[PassportNumber] [nvarchar](9) NOT NULL,
	[PassportCountryID] [int] NOT NULL,
	[BookingReference] [nvarchar](6) NOT NULL,
	[Confirmed] [bit] NOT NULL DEFAULT 1,
	[SeatNumber] [nvarchar](5) NULL,
 CONSTRAINT [PK_Ticket] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_Ticket_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([ID]),
 CONSTRAINT [FK_Ticket_Schedule] FOREIGN KEY ([ScheduleID]) REFERENCES [Schedules]([ID]),
 CONSTRAINT [FK_Ticket_Cabin] FOREIGN KEY ([CabinTypeID]) REFERENCES [CabinTypes]([ID]),
 CONSTRAINT [FK_Ticket_Country] FOREIGN KEY ([PassportCountryID]) REFERENCES [Countries]([ID])
)
GO

CREATE TABLE [dbo].[AmenitiesTickets](
	[AmenityID] [int] NOT NULL,
	[TicketID] [int] NOT NULL,
	[Price] [money] NOT NULL,
 CONSTRAINT [PK_AmenitiesTickets] PRIMARY KEY CLUSTERED ([AmenityID] ASC, [TicketID] ASC),
 CONSTRAINT [FK_AT_Amenity] FOREIGN KEY ([AmenityID]) REFERENCES [Amenities]([ID]),
 CONSTRAINT [FK_AT_Ticket] FOREIGN KEY ([TicketID]) REFERENCES [Tickets]([ID])
)
GO

CREATE TABLE [dbo].[LichSuTruyCap](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[ThoiGianDangNhap] [datetime] DEFAULT GETDATE(),
	[ThoiGianDangXuat] [datetime] NULL,
	[DiaChiIP] [varchar](50) NULL,
	[KetQua] [nvarchar](20) DEFAULT N'Thành công',
 CONSTRAINT [PK_LichSuTruyCap] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_LSTC_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([ID])
)
GO

CREATE TABLE [dbo].[LichSuChinhSua](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[HanhDong] [nvarchar](50) NULL,
	[DoiTuong] [nvarchar](100) NULL,
	[NoiDung] [nvarchar](500) NULL,
	[ThoiGian] [datetime] DEFAULT GETDATE(),
 CONSTRAINT [PK_LichSuChinhSua] PRIMARY KEY CLUSTERED ([ID] ASC),
 CONSTRAINT [FK_LSCS_User] FOREIGN KEY ([UserID]) REFERENCES [Users]([ID])
)
GO

-- =========================================================================
-- INDEXES CHO BẢNG SCHEDULES (LỊCH BAY)
-- Giúp tăng tốc độ tìm kiếm chuyến bay, đặc biệt với tính năng phân trang
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_Schedules_Date_Time' AND object_id = OBJECT_ID('Schedules'))
BEGIN
    CREATE NONCLUSTERED INDEX IDX_Schedules_Date_Time 
    ON [dbo].[Schedules] ([Date] ASC, [Time] ASC)
    INCLUDE ([FlightNumber], [AircraftID], [RouteID], [EconomyPrice], [Confirmed])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_Schedules_RouteID' AND object_id = OBJECT_ID('Schedules'))
BEGIN
    CREATE NONCLUSTERED INDEX IDX_Schedules_RouteID 
    ON [dbo].[Schedules] ([RouteID])
END
GO

-- =========================================================================
-- INDEXES CHO BẢNG TICKETS (VÉ)
-- Rất quan trọng khi đếm số ghế trống: COUNT(*) FROM Tickets WHERE ScheduleID = ...
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_Tickets_ScheduleID' AND object_id = OBJECT_ID('Tickets'))
BEGIN
    CREATE NONCLUSTERED INDEX IDX_Tickets_ScheduleID 
    ON [dbo].[Tickets] ([ScheduleID], [Confirmed])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_Tickets_BookingReference' AND object_id = OBJECT_ID('Tickets'))
BEGIN
    CREATE NONCLUSTERED INDEX IDX_Tickets_BookingReference 
    ON [dbo].[Tickets] ([BookingReference])
END
GO

-- =========================================================================
-- INDEXES CHO BẢNG ROUTES (TUYẾN BAY)
-- Hỗ trợ tốc độ khi JOIN hoặc WHERE theo sân bay đi/đến
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_Routes_Departure_Arrival' AND object_id = OBJECT_ID('Routes'))
BEGIN
    CREATE NONCLUSTERED INDEX IDX_Routes_Departure_Arrival 
    ON [dbo].[Routes] ([DepartureAirportID], [ArrivalAirportID])
    INCLUDE ([Distance], [FlightTime])
END
GO

-- =========================================================================
-- INDEXES CHO BẢNG USERS (NGƯỜI DÙNG)
-- Hỗ trợ tra cứu đăng nhập
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IDX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IDX_Users_Email 
    ON [dbo].[Users] ([Email])
    INCLUDE ([Password], [RoleID])
END
GO

PRINT N'Cài đặt Indexes thành công! Cơ sở dữ liệu đã được tối ưu tốc độ.'
GO
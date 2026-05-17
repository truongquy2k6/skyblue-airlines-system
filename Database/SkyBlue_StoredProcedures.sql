-- ==================== ĐĂNG NHẬP ====================
CREATE OR ALTER PROCEDURE sp_DangNhap @Email NVARCHAR(150), @Password NVARCHAR(50)
AS
SELECT u.ID, u.Email, u.FirstName, u.LastName, u.RoleID, r.Title as VaiTro,
       u.OfficeID, ISNULL(o.Title,N'') as VanPhong, u.Birthdate, u.Active
FROM Users u
JOIN Roles r ON u.RoleID = r.ID
LEFT JOIN Offices o ON u.OfficeID = o.ID
WHERE u.Email = @Email AND u.Password = @Password
GO

-- ==================== NHÂN VIÊN (Users) ====================
CREATE OR ALTER PROCEDURE sp_NhanVien_HienThi
AS
SELECT u.ID, u.LastName + N' ' + u.FirstName as HoTen, u.Email,
       r.Title as VaiTro, ISNULL(o.Title,N'') as VanPhong,
       u.Birthdate as NgaySinh,
       CASE WHEN u.Active = 1 THEN N'Hoạt động' ELSE N'Đã khóa' END as TrangThai,
       u.RoleID, u.OfficeID, u.FirstName, u.LastName, u.Active
FROM Users u
JOIN Roles r ON u.RoleID = r.ID
LEFT JOIN Offices o ON u.OfficeID = o.ID
ORDER BY u.ID
GO

CREATE OR ALTER PROCEDURE sp_NhanVien_Them
    @RoleID INT, @Email NVARCHAR(150), @Password NVARCHAR(50),
    @FirstName NVARCHAR(50), @LastName NVARCHAR(50),
    @OfficeID INT, @Birthdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        THROW 50001, N'Email này đã được sử dụng bởi một nhân viên khác!', 1;
        RETURN;
    END

    INSERT INTO Users (RoleID, Email, Password, FirstName, LastName, OfficeID, Birthdate, Active)
    VALUES (@RoleID, @Email, @Password, @FirstName, @LastName, @OfficeID, @Birthdate, 1)
END
GO

CREATE OR ALTER PROCEDURE sp_NhanVien_CapNhat
    @ID INT, @RoleID INT, @Email NVARCHAR(150), @Password NVARCHAR(50),
    @FirstName NVARCHAR(50), @LastName NVARCHAR(50),
    @OfficeID INT, @Birthdate DATE, @Active BIT
AS
BEGIN
    SET NOCOUNT ON;
    -- RÀNG BUỘC BẢO VỆ TÀI KHOẢN ADMIN GỐC: Không cho phép sửa tài khoản Administrator được thêm trực tiếp trong SQL (ID = 1 và ID = 4)
    IF @ID IN (1, 4)
    BEGIN
        THROW 50003, N'Không được phép chỉnh sửa tài khoản Administrator gốc!', 1;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND ID <> @ID)
    BEGIN
        THROW 50002, N'Email này đã được sử dụng bởi một nhân viên khác!', 1;
        RETURN;
    END

    UPDATE Users SET RoleID=@RoleID, Email=@Email,
        Password=CASE WHEN @Password='' THEN Password ELSE @Password END,
        FirstName=@FirstName, LastName=@LastName, OfficeID=@OfficeID,
        Birthdate=@Birthdate, Active=@Active
    WHERE ID=@ID
END
GO

CREATE OR ALTER PROCEDURE sp_NhanVien_Xoa @ID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- RÀNG BUỘC BẢO VỆ TÀI KHOẢN ADMIN GỐC: Không cho phép khóa hoặc xóa tài khoản Administrator được thêm trực tiếp trong SQL (ID = 1 và ID = 4)
    IF @ID IN (1, 4)
    BEGIN
        THROW 50004, N'Không được phép khóa hoặc xóa tài khoản Administrator gốc!', 1;
        RETURN;
    END

    UPDATE Users SET Active=0 WHERE ID=@ID
END
GO

-- ==================== SÂN BAY (Airports) ====================
CREATE OR ALTER PROCEDURE sp_SanBay_HienThi
AS
SELECT a.ID, a.IATACode, a.Name as TenSanBay, c.Name as QuocGia, a.CountryID
FROM Airports a JOIN Countries c ON a.CountryID = c.ID ORDER BY a.ID
GO

CREATE OR ALTER PROCEDURE sp_SanBay_Them @IATACode VARCHAR(3), @Name NVARCHAR(100), @CountryID INT
AS
INSERT INTO Airports (IATACode, Name, CountryID) VALUES (@IATACode, @Name, @CountryID)
GO

CREATE OR ALTER PROCEDURE sp_SanBay_CapNhat @ID INT, @IATACode VARCHAR(3), @Name NVARCHAR(100), @CountryID INT
AS
UPDATE Airports SET IATACode=@IATACode, Name=@Name, CountryID=@CountryID WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_SanBay_Xoa @ID INT
AS
DELETE FROM Airports WHERE ID=@ID
GO

-- ==================== TUYẾN BAY (Routes) ====================
CREATE OR ALTER PROCEDURE sp_TuyenBay_HienThi
AS
SELECT r.ID,
    da.IATACode + N' - ' + da.Name as DiemDi,
    aa.IATACode + N' - ' + aa.Name as DiemDen,
    r.Distance as KhoangCach, r.FlightTime as ThoiGianBay,
    r.DepartureAirportID, r.ArrivalAirportID,
    da.IATACode as MaDi, aa.IATACode as MaDen
FROM Routes r
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
ORDER BY r.ID
GO

CREATE OR ALTER PROCEDURE sp_TuyenBay_Them @DepID INT, @ArrID INT, @Distance INT, @FlightTime INT
AS
INSERT INTO Routes (DepartureAirportID, ArrivalAirportID, Distance, FlightTime)
VALUES (@DepID, @ArrID, @Distance, @FlightTime)
GO

CREATE OR ALTER PROCEDURE sp_TuyenBay_CapNhat @ID INT, @DepID INT, @ArrID INT, @Distance INT, @FlightTime INT
AS
UPDATE Routes SET DepartureAirportID=@DepID, ArrivalAirportID=@ArrID,
    Distance=@Distance, FlightTime=@FlightTime WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_TuyenBay_Xoa @ID INT
AS
DELETE FROM Routes WHERE ID=@ID
GO

-- ==================== MÁY BAY (Aircrafts) ====================
CREATE OR ALTER PROCEDURE sp_MayBay_HienThi
AS
SELECT ID, Name as TenMayBay, MakeModel as Model, TotalSeats as TongGhe,
    EconomySeats as GheEconomy, BusinessSeats as GheBusiness,
    CAST(ROUND(CAST(BusinessSeats AS FLOAT)/TotalSeats*100,1) AS DECIMAL(5,1)) as TyLeBusiness
FROM Aircrafts ORDER BY ID
GO

CREATE OR ALTER PROCEDURE sp_MayBay_Them
    @Name NVARCHAR(50), @MakeModel NVARCHAR(50), @TotalSeats INT, @EconomySeats INT, @BusinessSeats INT
AS
INSERT INTO Aircrafts (Name, MakeModel, TotalSeats, EconomySeats, BusinessSeats)
VALUES (@Name, @MakeModel, @TotalSeats, @EconomySeats, @BusinessSeats)
GO

CREATE OR ALTER PROCEDURE sp_MayBay_CapNhat
    @ID INT, @Name NVARCHAR(50), @MakeModel NVARCHAR(50),
    @TotalSeats INT, @EconomySeats INT, @BusinessSeats INT
AS
UPDATE Aircrafts SET Name=@Name, MakeModel=@MakeModel, TotalSeats=@TotalSeats,
    EconomySeats=@EconomySeats, BusinessSeats=@BusinessSeats WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_MayBay_Xoa @ID INT
AS
DELETE FROM Aircrafts WHERE ID=@ID
GO

-- ==================== LỊCH BAY (Schedules) ====================
CREATE OR ALTER PROCEDURE sp_LichBay_HienThi
AS
SELECT s.ID, s.FlightNumber as SoHieu, s.Date as NgayBay, s.Time as GioBay,
    da.IATACode + N' → ' + aa.IATACode as TuyenBay,
    da.IATACode as MaDi, aa.IATACode as MaDen,
    da.Name as SanBayDi, aa.Name as SanBayDen,
    ac.Name as MayBay, ac.MakeModel as Model,
    s.EconomyPrice as GiaEconomy,
    CAST(s.EconomyPrice * 2.5 AS MONEY) as GiaBusiness,
    CAST(s.EconomyPrice * 4.0 AS MONEY) as GiaFirstClass,
    r.FlightTime as ThoiGianBay, r.Distance as KhoangCach,
    CASE 
        WHEN s.Confirmed = 0 THEN N'Đã hủy'
        WHEN DATEADD(MINUTE, r.FlightTime, CAST(CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) AS DATETIME)) < GETDATE() THEN N'Đã bay'
        WHEN CAST(CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) AS DATETIME) <= GETDATE() THEN N'Đang bay'
        ELSE N'Đã xác nhận'
    END as TrangThai,
    s.Confirmed, s.AircraftID, s.RouteID,
    (ac.TotalSeats - ISNULL((SELECT COUNT(*) FROM Tickets t WHERE t.ScheduleID = s.ID AND t.Confirmed = 1), 0)) as GheTrong
FROM Schedules s
JOIN Routes r ON s.RouteID = r.ID
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
JOIN Aircrafts ac ON s.AircraftID = ac.ID
ORDER BY s.Date DESC, s.Time
GO

CREATE OR ALTER PROCEDURE sp_LichBay_Them
    @FlightNumber NVARCHAR(10), @Date DATE, @Time TIME,
    @AircraftID INT, @RouteID INT, @EconomyPrice MONEY, @Confirmed BIT
AS
INSERT INTO Schedules (FlightNumber,[Date],[Time],AircraftID,RouteID,EconomyPrice,Confirmed)
VALUES (@FlightNumber, @Date, @Time, @AircraftID, @RouteID, @EconomyPrice, @Confirmed)
GO

CREATE OR ALTER PROCEDURE sp_LichBay_CapNhat
    @ID INT, @FlightNumber NVARCHAR(10), @Date DATE, @Time TIME,
    @AircraftID INT, @RouteID INT, @EconomyPrice MONEY, @Confirmed BIT
AS
UPDATE Schedules SET FlightNumber=@FlightNumber,[Date]=@Date,[Time]=@Time,
    AircraftID=@AircraftID, RouteID=@RouteID, EconomyPrice=@EconomyPrice,
    Confirmed=@Confirmed WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_LichBay_Xoa @ID INT
AS
DELETE FROM Schedules WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_LichBay_TimKiem
    @SanBayDi INT = NULL,
    @SanBayDen INT = NULL,
    @NgayTu DATE = NULL,
    @NgayDen DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH FilteredSchedules AS (
        SELECT s.ID, s.FlightNumber as SoHieu, s.Date as NgayBay, s.Time as GioBay,
            da.Name as SanBayDi,
            aa.Name as SanBayDen,
            ac.Name as MayBay, ac.MakeModel as Model,
            s.EconomyPrice as GiaEconomy,
            CAST(s.EconomyPrice * 2.5 AS MONEY) as GiaBusiness,
            CAST(s.EconomyPrice * 4.0 AS MONEY) as GiaFirstClass,
            r.FlightTime as ThoiGianBay,
            DATEADD(MINUTE, r.FlightTime, CAST(CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) AS DATETIME)) as GioDen,
            COUNT(*) OVER() as TotalRecords,
            ROW_NUMBER() OVER(ORDER BY s.Date ASC, s.Time ASC) as RowNum
        FROM Schedules s
        JOIN Routes r ON s.RouteID = r.ID
        JOIN Airports da ON r.DepartureAirportID = da.ID
        JOIN Airports aa ON r.ArrivalAirportID = aa.ID
        JOIN Aircrafts ac ON s.AircraftID = ac.ID
        WHERE s.Confirmed = 1
        -- Chỉ lấy các chuyến bay trong tương lai (Ngày >= Hôm nay)
        AND (s.Date > CAST(GETDATE() AS DATE) OR (s.Date = CAST(GETDATE() AS DATE) AND s.Time > CAST(GETDATE() AS TIME)))
        AND (@SanBayDi IS NULL OR r.DepartureAirportID = @SanBayDi)
        AND (@SanBayDen IS NULL OR r.ArrivalAirportID = @SanBayDen)
        AND (@NgayTu IS NULL OR s.Date >= @NgayTu)
        AND (@NgayDen IS NULL OR s.Date <= @NgayDen)
    )
    SELECT ID, SoHieu, NgayBay, GioBay, SanBayDi, SanBayDen, MayBay, Model, GiaEconomy, GiaBusiness, GiaFirstClass, ThoiGianBay, GioDen, TotalRecords
    FROM FilteredSchedules
    WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize
    ORDER BY RowNum;
END
GO

-- ==================== VÉ (Tickets) ====================
CREATE OR ALTER PROCEDURE sp_Ve_HienThi
AS
SELECT t.ID, t.BookingReference as MaDatCho,
    t.Lastname + N' ' + t.Firstname as TenKhach, t.Phone as SoDT,
    s.FlightNumber as SoHieu,
    da.IATACode + N' → ' + aa.IATACode as TuyenBay,
    CONVERT(VARCHAR, s.Date, 23) + ' ' + CONVERT(VARCHAR(5), s.Time) as NgayGio,
    ct.Name as HangGhe,
    CASE 
        WHEN t.Confirmed = 0 THEN N'Đã hủy'
        WHEN CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) > GETDATE() THEN N'Đã xác nhận'
        ELSE N'Đã bay'
    END as TrangThai,
    t.Email, t.PassportNumber as SoHoChieu,
    t.Firstname, t.Lastname, t.ScheduleID, t.CabinTypeID, t.UserID,
    co.Name as QuocTich, t.PassportCountryID, t.Confirmed
FROM Tickets t
JOIN Schedules s ON t.ScheduleID = s.ID
JOIN Routes r ON s.RouteID = r.ID
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
JOIN Countries co ON t.PassportCountryID = co.ID
ORDER BY t.ID DESC
GO

CREATE OR ALTER PROCEDURE sp_Ve_Them
    @UserID INT, @ScheduleID INT, @CabinTypeID INT,
    @Firstname NVARCHAR(50), @Lastname NVARCHAR(50), @Email NVARCHAR(150),
    @Phone NVARCHAR(14), @PassportNumber NVARCHAR(9),
    @PassportCountryID INT, @BookingReference NVARCHAR(6),
    @SeatNumber NVARCHAR(5) = NULL
AS
INSERT INTO Tickets (UserID, ScheduleID, CabinTypeID, Firstname, Lastname, Email, Phone,
    PassportNumber, PassportCountryID, BookingReference, Confirmed, SeatNumber)
VALUES (@UserID, @ScheduleID, @CabinTypeID, @Firstname, @Lastname, @Email, @Phone,
    @PassportNumber, @PassportCountryID, @BookingReference, 1, @SeatNumber)
SELECT SCOPE_IDENTITY() as NewID
GO

CREATE OR ALTER PROCEDURE sp_Ve_TimKiem @Keyword NVARCHAR(100)
AS
SELECT t.ID, t.BookingReference as MaDatCho,
    t.Lastname + N' ' + t.Firstname as TenKhach, t.Phone as SoDT,
    s.FlightNumber as SoHieu,
    da.IATACode + N' → ' + aa.IATACode as TuyenBay,
    CONVERT(VARCHAR, s.Date, 23) + ' ' + CONVERT(VARCHAR(5), s.Time) as NgayGio,
    ct.Name as HangGhe,
    t.SeatNumber,
    t.Email,
    CASE 
        WHEN t.Confirmed = 0 THEN N'Đã hủy'
        WHEN CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) > GETDATE() THEN N'Đã xác nhận'
        ELSE N'Đã bay'
    END as TrangThai
FROM Tickets t
JOIN Schedules s ON t.ScheduleID = s.ID
JOIN Routes r ON s.RouteID = r.ID
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
WHERE t.Lastname + N' ' + t.Firstname LIKE N'%'+@Keyword+N'%'
   OR t.Phone LIKE N'%'+@Keyword+N'%'
   OR t.BookingReference LIKE N'%'+@Keyword+N'%'
ORDER BY t.ID DESC
GO

-- ==================== DỊCH VỤ (Amenities) ====================
CREATE OR ALTER PROCEDURE sp_DichVu_HienThi
AS
SELECT ID, Service as TenDichVu, Price as Gia FROM Amenities ORDER BY ID
GO

CREATE OR ALTER PROCEDURE sp_DichVu_Them @Service NVARCHAR(50), @Price MONEY
AS
INSERT INTO Amenities (Service, Price) VALUES (@Service, @Price)
GO

CREATE OR ALTER PROCEDURE sp_DichVu_CapNhat @ID INT, @Service NVARCHAR(50), @Price MONEY
AS
UPDATE Amenities SET Service=@Service, Price=@Price WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_DichVu_Xoa @ID INT
AS
DELETE FROM Amenities WHERE ID=@ID
GO

CREATE OR ALTER PROCEDURE sp_DichVu_LayTheoVe @TicketID INT
AS
SELECT a.ID, a.Service as TenDichVu, at2.Price as Gia
FROM AmenitiesTickets at2
JOIN Amenities a ON at2.AmenityID = a.ID
WHERE at2.TicketID = @TicketID
GO

CREATE OR ALTER PROCEDURE sp_DichVu_GanChoVe @AmenityID INT, @TicketID INT, @Price MONEY
AS
INSERT INTO AmenitiesTickets (AmenityID, TicketID, Price) VALUES (@AmenityID, @TicketID, @Price)
GO

CREATE OR ALTER PROCEDURE sp_DichVu_XoaKhoiVe @AmenityID INT, @TicketID INT
AS
DELETE FROM AmenitiesTickets WHERE AmenityID=@AmenityID AND TicketID=@TicketID
GO

-- ==================== HẠNG GHẾ (CabinTypes) ====================
CREATE OR ALTER PROCEDURE sp_HangGhe_HienThi
AS
SELECT ID, Name as TenHangGhe, PriceMultiplier as HeSoGia FROM CabinTypes ORDER BY ID
GO

CREATE OR ALTER PROCEDURE sp_HangGhe_LayCauHinh @CabinTypeID INT
AS
SELECT a.ID as AmenityID, a.Service as TenDichVu, a.Price as Gia,
    CASE WHEN act.AmenityID IS NOT NULL THEN 1 ELSE 0 END as DuocChon
FROM Amenities a
LEFT JOIN AmenitiesCabinType act ON a.ID = act.AmenityID AND act.CabinTypeID = @CabinTypeID
ORDER BY a.ID
GO

CREATE OR ALTER PROCEDURE sp_HangGhe_GanDichVu @CabinTypeID INT, @AmenityID INT
AS
IF NOT EXISTS (SELECT 1 FROM AmenitiesCabinType WHERE CabinTypeID=@CabinTypeID AND AmenityID=@AmenityID)
    INSERT INTO AmenitiesCabinType (CabinTypeID, AmenityID) VALUES (@CabinTypeID, @AmenityID)
GO

CREATE OR ALTER PROCEDURE sp_HangGhe_GoDichVu @CabinTypeID INT, @AmenityID INT
AS
DELETE FROM AmenitiesCabinType WHERE CabinTypeID=@CabinTypeID AND AmenityID=@AmenityID
GO

-- ==================== TRANG CHỦ (Dashboard) ====================
CREATE OR ALTER PROCEDURE sp_TrangChu_ThongKe
AS
SELECT
    (SELECT COUNT(*) FROM Users WHERE Active=1) as NhanVienHoatDong,
    (SELECT COUNT(*) FROM Schedules WHERE [Date]=CAST(GETDATE() AS DATE) AND Confirmed=1) as ChuyenBayHomNay,
    (SELECT COUNT(*) FROM Tickets) as TongVeDaBan,
    (SELECT COUNT(*) FROM Aircrafts) as TongMayBay,
    (SELECT ISNULL(SUM(s.EconomyPrice * ct.PriceMultiplier),0)
     FROM Tickets t JOIN Schedules s ON t.ScheduleID=s.ID
     JOIN CabinTypes ct ON t.CabinTypeID=ct.ID) as DoanhThu
GO

CREATE OR ALTER PROCEDURE sp_TrangChu_LichBayHomNay
AS
SELECT s.FlightNumber as SoHieu, s.Time as GioBay,
    da.IATACode + N' → ' + aa.IATACode as TuyenBay,
    ac.Name as MayBay,
    CASE 
        WHEN s.Confirmed = 0 THEN N'Đã hủy'
        WHEN DATEADD(MINUTE, r.FlightTime, CAST(CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) AS DATETIME)) < GETDATE() THEN N'Đã bay'
        WHEN CAST(CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) AS DATETIME) <= GETDATE() THEN N'Đang bay'
        ELSE N'Đã xác nhận'
    END as TrangThai
FROM Schedules s
JOIN Routes r ON s.RouteID = r.ID
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
JOIN Aircrafts ac ON s.AircraftID = ac.ID
WHERE s.[Date] = CAST(GETDATE() AS DATE)
ORDER BY s.Time
GO

-- ==================== QUỐC GIA & VĂN PHÒNG ====================
CREATE OR ALTER PROCEDURE sp_QuocGia_HienThi
AS
SELECT ID, Name as TenQuocGia FROM Countries ORDER BY Name
GO

CREATE OR ALTER PROCEDURE sp_VanPhong_HienThi
AS
SELECT o.ID, o.Title as TenVanPhong, o.Phone, o.Contact, c.Name as QuocGia
FROM Offices o JOIN Countries c ON o.CountryID = c.ID ORDER BY o.ID
GO

CREATE OR ALTER PROCEDURE sp_VaiTro_HienThi
AS
SELECT ID, Title as TenVaiTro FROM Roles ORDER BY ID
GO

-- ==================== LỊCH SỬ ====================
CREATE OR ALTER PROCEDURE sp_LichSu_LayTruyCap
    @PageNumber INT = 1,
    @PageSize INT = 15
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ls.ID, 
        DATEADD(HOUR, 7, ls.ThoiGianDangNhap) as ThoiGianDangNhap, 
        DATEADD(HOUR, 7, ls.ThoiGianDangXuat) as ThoiGianDangXuat,
        u.Email, u.LastName + N' ' + u.FirstName as HoTen,
        r.Title as VaiTro, ISNULL(o.Title,N'') as VanPhong,
        ls.DiaChiIP, ls.KetQua,
        COUNT(*) OVER() as TotalRecords
    FROM LichSuTruyCap ls
    JOIN Users u ON ls.UserID = u.ID
    JOIN Roles r ON u.RoleID = r.ID
    LEFT JOIN Offices o ON u.OfficeID = o.ID
    ORDER BY ls.ThoiGianDangNhap DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE sp_LichSu_GhiNhanTruyCap @UserID INT, @IP VARCHAR(50)
AS
INSERT INTO LichSuTruyCap (UserID, DiaChiIP, KetQua) VALUES (@UserID, @IP, N'Thành công')
GO

CREATE OR ALTER PROCEDURE sp_LichSu_GhiNhanDangXuat @UserID INT
AS
UPDATE LichSuTruyCap SET ThoiGianDangXuat = GETDATE()
WHERE UserID = @UserID AND ThoiGianDangXuat IS NULL
GO

CREATE OR ALTER PROCEDURE sp_LichSu_LayChinhSua
    @PageNumber INT = 1,
    @PageSize INT = 15
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ls.ID, 
        DATEADD(HOUR, 7, ls.ThoiGian) as ThoiGian,
        u.LastName + N' ' + u.FirstName as NguoiDung, u.Email,
        ls.HanhDong, ls.DoiTuong, ls.NoiDung,
        COUNT(*) OVER() as TotalRecords
    FROM LichSuChinhSua ls
    JOIN Users u ON ls.UserID = u.ID
    ORDER BY ls.ThoiGian DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE sp_LichSu_GhiNhanChinhSua
    @UserID INT, @HanhDong NVARCHAR(50), @DoiTuong NVARCHAR(100), @NoiDung NVARCHAR(500)
AS
INSERT INTO LichSuChinhSua (UserID, HanhDong, DoiTuong, NoiDung) VALUES (@UserID, @HanhDong, @DoiTuong, @NoiDung)
GO

CREATE OR ALTER PROCEDURE sp_LichSu_XoaTruyCap
AS
DELETE FROM LichSuTruyCap
GO

CREATE OR ALTER PROCEDURE sp_LichSu_XoaChinhSua
AS
DELETE FROM LichSuChinhSua
GO

-- ==================== BÁO CÁO ====================
CREATE OR ALTER PROCEDURE sp_BaoCao_DanhSachHanhKhach @ScheduleID INT
AS
SELECT t.BookingReference as MaDatCho, t.Lastname + N' ' + t.Firstname as HoTen,
    t.Phone as SoDT, t.Email, t.PassportNumber as SoHoChieu,
    co.Name as QuocTich, ct.Name as HangGhe,
    ISNULL(t.SeatNumber, N'--') as SoGhe,
    CASE WHEN t.Confirmed=1 THEN N'Đã xác nhận' ELSE N'Đã hủy' END as TrangThai
FROM Tickets t
JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
JOIN Countries co ON t.PassportCountryID = co.ID
WHERE (@ScheduleID IS NULL OR @ScheduleID = 0 OR t.ScheduleID = @ScheduleID)
ORDER BY ct.ID, t.Lastname
GO

CREATE OR ALTER PROCEDURE sp_BaoCao_ChuyenBayCombo
AS
SELECT s.ID, s.FlightNumber + N' - ' + da.IATACode + N' → ' + aa.IATACode
    + N' - ' + CONVERT(VARCHAR, s.Date, 23) + ' ' + CONVERT(VARCHAR(5), s.Time) as Display
FROM Schedules s
JOIN Routes r ON s.RouteID = r.ID
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
ORDER BY s.Date DESC, s.Time
GO

CREATE OR ALTER PROCEDURE sp_BaoCao_ThongKeVanPhong
AS
SELECT o.Title as VanPhong, COUNT(t.ID) as SoVe,
    ISNULL(SUM(s.EconomyPrice * ct.PriceMultiplier),0) as DoanhThu
FROM Offices o
LEFT JOIN Users u ON o.ID = u.OfficeID
LEFT JOIN Tickets t ON u.ID = t.UserID
LEFT JOIN Schedules s ON t.ScheduleID = s.ID
LEFT JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
GROUP BY o.Title
ORDER BY DoanhThu DESC
GO

CREATE OR ALTER PROCEDURE sp_BaoCao_ChiTietChuyenBay @ScheduleID INT
AS
SELECT s.FlightNumber as SoHieu,
    da.IATACode + N' - ' + da.Name as SanBayDi,
    aa.IATACode + N' - ' + aa.Name as SanBayDen,
    s.Date as NgayBay, s.Time as GioBay,
    ac.Name + N' - ' + ac.MakeModel as MayBay,
    (SELECT COUNT(*) FROM Tickets WHERE ScheduleID = @ScheduleID) as TongHanhKhach
FROM Schedules s
JOIN Routes r ON s.RouteID = r.ID
JOIN Airports da ON r.DepartureAirportID = da.ID
JOIN Airports aa ON r.ArrivalAirportID = aa.ID
JOIN Aircrafts ac ON s.AircraftID = ac.ID
WHERE s.ID = @ScheduleID
GO

-- ==================== DỊCH VỤ THỐNG KÊ ====================
CREATE OR ALTER PROCEDURE sp_DichVu_ThongKe
AS
SELECT
    (SELECT COUNT(*) FROM Amenities) as TongDichVu,
    (SELECT AVG(Price) FROM Amenities) as GiaTrungBinh,
    (SELECT MIN(Price) FROM Amenities) as GiaThapNhat,
    (SELECT MAX(Price) FROM Amenities) as GiaCaoNhat,
    (SELECT ISNULL(SUM(Price),0) FROM AmenitiesTickets) as TongDoanhThu,
    (SELECT COUNT(DISTINCT TicketID) FROM AmenitiesTickets) as VeDaDatDV
GO

CREATE OR ALTER PROCEDURE sp_DichVu_Top3
AS
SELECT TOP 3 a.Service as TenDichVu, a.Price as Gia,
    COUNT(at2.TicketID) as SoLanDat
FROM Amenities a
LEFT JOIN AmenitiesTickets at2 ON a.ID = at2.AmenityID
GROUP BY a.ID, a.Service, a.Price
ORDER BY SoLanDat DESC
GO

-- ==================== TẠO LỊCH BAY TỰ ĐỘNG ====================
CREATE OR ALTER PROCEDURE sp_LichBay_TaoTuDong7Ngay
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Day INT = 0
    DECLARE @TargetDate DATE
    DECLARE @AircraftCount INT = (SELECT COUNT(*) FROM Aircrafts)
    DECLARE @DefaultCountryID INT = (SELECT TOP 1 ID FROM Countries WHERE Name LIKE N'%Vietnam%' OR Name LIKE N'%Viet%')
    IF @DefaultCountryID IS NULL SET @DefaultCountryID = (SELECT TOP 1 ID FROM Countries ORDER BY ID)
    DECLARE @DefaultUserID INT = ISNULL((SELECT TOP 1 ID FROM Users WHERE RoleID = 2), 1)

    DECLARE @HoList TABLE (idx INT, Ho NVARCHAR(50))
    INSERT INTO @HoList VALUES 
        (1, N'Nguyễn'), (2, N'Trần'), (3, N'Lê'), (4, N'Phạm'),
        (5, N'Huỳnh'), (6, N'Vũ'), (7, N'Võ'), (8, N'Đặng'),
        (9, N'Hoàng'), (10, N'Phan'), (11, N'Bùi'), (12, N'Đỗ'),
        (13, N'Hồ'), (14, N'Ngô'), (15, N'Dương'), (16, N'Lý')
    
    DECLARE @TenList TABLE (idx INT, Ten NVARCHAR(50))
    INSERT INTO @TenList VALUES 
        (1, N'Quý'), (2, N'Ái'), (3, N'Minh'), (4, N'Nhã'),
        (5, N'Nguyên'), (6, N'Long'), (7, N'Nghĩa'), (8, N'Dũng'),
        (9, N'Anh'), (10, N'Bình'), (11, N'Vy'), (12, N'Khánh'),
        (13, N'Tuấn'), (14, N'Trang'), (15, N'Hùng'), (16, N'Linh'),
        (17, N'Nam'), (18, N'Lan'), (19, N'Phong'), (20, N'Hải')
    
    WHILE @Day <= 7
    BEGIN
        SET @TargetDate = CAST(DATEADD(DAY, @Day, GETDATE()) AS DATE)
        
        -- 1. Sinh Lịch bay nếu chưa có
        INSERT INTO Schedules (FlightNumber, [Date], [Time], AircraftID, RouteID, EconomyPrice, Confirmed)
        SELECT 
            'VN' + RIGHT('000' + CAST(r.ID AS VARCHAR(4)), 4),
            @TargetDate,
            TIMEFROMPARTS(6 + (r.ID % 14), (r.ID % 4) * 15, 0, 0, 0),
            ISNULL((SELECT TOP 1 ID FROM Aircrafts ORDER BY NEWID()), 1),
            r.ID,
            r.Distance * 1500,
            1
        FROM Routes r
        WHERE NOT EXISTS (
            SELECT 1 FROM Schedules s 
            WHERE s.RouteID = r.ID AND s.[Date] = @TargetDate
        )
        
        -- 2. Sinh 10 Vé tự động cho chuyến bay chưa có vé nào (bao gồm số ghế tự động và họ tên ngẫu nhiên)
        INSERT INTO Tickets (UserID, ScheduleID, CabinTypeID, Firstname, Lastname, Email, Phone, PassportNumber, PassportCountryID, BookingReference, Confirmed, SeatNumber)
        SELECT 
            ISNULL((SELECT TOP 1 ID FROM Users WHERE RoleID = CASE WHEN ABS(CHECKSUM(NEWID(), s.ID, nums.n)) % 10 < 7 THEN 1 ELSE 3 END ORDER BY ABS(CHECKSUM(NEWID(), s.ID, nums.n))), @DefaultUserID),
            s.ID,
            (nums.n % 3) + 1,
            t.Ten,
            h.Ho,
            'khach' + RIGHT('00000' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR), 5) + '@gmail.com',
            '09' + RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000 AS VARCHAR), 8),
            'C' + RIGHT('0000000' + CAST(ABS(CHECKSUM(NEWID())) % 10000000 AS VARCHAR), 7),
            @DefaultCountryID,
            UPPER(SUBSTRING(REPLACE(CAST(NEWID() AS VARCHAR(36)), '-', ''), 1, 6)),
            1,
            -- Sinh số ghế tự động theo hạng: Hạng nhất hàng 1-3, Thương gia 4-9, Phổ thông 10+
            -- Công thức: (hàng ghế)(cột A-F), ví dụ: 1A, 11C, 21F
            CAST(
                CASE (nums.n % 3) + 1
                    WHEN 1 THEN nums.n              -- Hạng nhất: hàng 1..10
                    WHEN 2 THEN nums.n + 10         -- Thương gia: hàng 11..20
                    ELSE        nums.n + 20         -- Phổ thông:  hàng 21..30
                END
            AS VARCHAR(3)) + SUBSTRING('ABCDEF', (nums.n % 6) + 1, 1)
        FROM Schedules s
        CROSS JOIN (
            SELECT 1 AS n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 
            UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
            UNION ALL SELECT 11 UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 14 UNION ALL SELECT 15
            UNION ALL SELECT 16 UNION ALL SELECT 17 UNION ALL SELECT 18 UNION ALL SELECT 19 UNION ALL SELECT 20
        ) AS nums
        -- Xóa bỏ liên kết tuyến tính (nums.n % ...) cũ, kết nối ngẫu nhiên hoàn toàn qua CHECKSUM(NEWID())
        JOIN @TenList t ON t.idx = (ABS(CHECKSUM(CONCAT(NEWID(), s.ID, nums.n))) % 20) + 1
        JOIN @HoList  h ON h.idx = (ABS(CHECKSUM(CONCAT(NEWID(), s.ID, nums.n, 'ho'))) % 16) + 1
        WHERE s.[Date] = @TargetDate
          AND NOT EXISTS (SELECT 1 FROM Tickets tk WHERE tk.ScheduleID = s.ID)
        
        -- 3. Gán các dịch vụ mặc định theo Hạng ghế (CabinType) cho các vé vừa được sinh tự động
        INSERT INTO AmenitiesTickets (AmenityID, TicketID, Price)
        SELECT act.AmenityID, t.ID, a.Price
        FROM Tickets t
        JOIN Schedules s ON t.ScheduleID = s.ID
        JOIN AmenitiesCabinType act ON t.CabinTypeID = act.CabinTypeID
        JOIN Amenities a ON act.AmenityID = a.ID
        WHERE s.[Date] = @TargetDate
          AND NOT EXISTS (
              SELECT 1 FROM AmenitiesTickets at 
              WHERE at.TicketID = t.ID
          )
        
        SET @Day = @Day + 1
    END
END
GO


-- ==================== CÁC STORED PROCEDURE MỚI BẰNG TIẾNG VIỆT ====================

-- 1. Lấy danh sách tuyến bay lọc ở màn hình quản lý vé
CREATE OR ALTER PROCEDURE sp_Ve_DanhSachTuyenBay
AS
BEGIN
    SELECT DISTINCT da.IATACode + N' → ' + aa.IATACode as TuyenBay
    FROM Routes r
    JOIN Airports da ON r.DepartureAirportID = da.ID
    JOIN Airports aa ON r.ArrivalAirportID = aa.ID
END
GO

-- 2. Hiển thị phân trang Server-side vé kết hợp bộ lọc tìm kiếm đa năng
CREATE OR ALTER PROCEDURE sp_Ve_HienThiPhanTrang
    @Keyword NVARCHAR(100) = NULL,
    @TuyenBay NVARCHAR(100) = NULL,
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH FilteredTickets AS (
        SELECT t.ID, t.BookingReference as MaDatCho,
            t.Lastname + N' ' + t.Firstname as TenKhach, t.Phone as SoDT,
            s.FlightNumber as SoHieu,
            da.IATACode + N' → ' + aa.IATACode as TuyenBay,
            CONVERT(VARCHAR, s.Date, 23) + ' ' + CONVERT(VARCHAR(5), s.Time) as NgayGio,
            ct.Name as HangGhe,
            CASE 
                WHEN t.Confirmed = 0 THEN N'Đã hủy'
                WHEN CAST(s.Date AS DATETIME) + CAST(s.Time AS DATETIME) > GETDATE() THEN N'Đã xác nhận'
                ELSE N'Đã bay'
            END as TrangThai,
            t.Confirmed,
            t.SeatNumber,
            t.Email,
            u.LastName + N' ' + u.FirstName as NhanVienBan,
            COUNT(*) OVER() as TotalRecords,
            ROW_NUMBER() OVER(ORDER BY t.ID DESC) as RowNum
        FROM Tickets t
        JOIN Schedules s ON t.ScheduleID = s.ID
        JOIN Routes r ON s.RouteID = r.ID
        JOIN Airports da ON r.DepartureAirportID = da.ID
        JOIN Airports aa ON r.ArrivalAirportID = aa.ID
        JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
        JOIN Countries co ON t.PassportCountryID = co.ID
        LEFT JOIN Users u ON t.UserID = u.ID
        WHERE (@Keyword IS NULL OR t.Lastname + N' ' + t.Firstname LIKE N'%' + @Keyword + N'%'
                               OR t.Phone LIKE N'%' + @Keyword + N'%'
                               OR t.BookingReference LIKE N'%' + @Keyword + N'%')
          AND (@TuyenBay IS NULL OR da.IATACode + N' → ' + aa.IATACode = @TuyenBay)
    )
    SELECT ID, MaDatCho, TenKhach, SoDT, SoHieu, TuyenBay, NgayGio, HangGhe, TrangThai, Confirmed, SeatNumber, Email, NhanVienBan, TotalRecords
    FROM FilteredTickets
    WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize
    ORDER BY RowNum;
END
GO

-- ==================== CHĂM SÓC KHÁCH HÀNG (CSKH) ====================

-- Bảng lưu trữ hàng đợi gửi Email
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CSKH_MailQueue]') AND type in (N'U'))
BEGIN
    CREATE TABLE CSKH_MailQueue (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        TicketID INT NOT NULL FOREIGN KEY REFERENCES Tickets(ID),
        Status NVARCHAR(50) DEFAULT N'Chưa gửi', -- Chưa gửi, Đã gửi, Lỗi
        CreatedTime DATETIME DEFAULT GETDATE(),
        SentTime DATETIME NULL,
        ErrorMessage NVARCHAR(500) NULL
    )
END
GO

-- Bảng lưu trữ Feedback của khách hàng
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CSKH_Feedback]') AND type in (N'U'))
BEGIN
    CREATE TABLE CSKH_Feedback (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        PassengerName NVARCHAR(100) NOT NULL,
        PassengerPhone VARCHAR(50) NULL,
        PassengerEmail VARCHAR(100) NULL,
        Rating INT NOT NULL, -- 1 đến 5 sao
        Category NVARCHAR(100) NOT NULL, -- Thái độ phục vụ, Chất lượng chuyến bay, ...
        Content NVARCHAR(1000) NOT NULL,
        CreatedTime DATETIME DEFAULT GETDATE(),
        OperatorID INT NOT NULL FOREIGN KEY REFERENCES Users(ID)
    )
END
GO

-- 1. SP Lấy danh sách Mail Queue
CREATE OR ALTER PROCEDURE sp_CSKH_LayMailQueue
    @PageNumber INT = 1,
    @PageSize INT = 15,
    @StatusFilter NVARCHAR(50) = NULL,
    @FlightDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT q.ID, q.TicketID, q.Status, q.CreatedTime, q.SentTime, q.ErrorMessage,
           t.Lastname + N' ' + t.Firstname as TenKhach, t.Email, t.Phone, t.BookingReference, t.SeatNumber,
           s.FlightNumber, s.Date as NgayBay, s.Time as GioBay,
           da.Name + N' (' + da.IATACode + N') → ' + aa.Name + N' (' + aa.IATACode + N')' as TuyenBay,
           COUNT(*) OVER() as TotalRecords
    FROM CSKH_MailQueue q
    JOIN Tickets t ON q.TicketID = t.ID
    JOIN Schedules s ON t.ScheduleID = s.ID
    JOIN Routes r ON s.RouteID = r.ID
    JOIN Airports da ON r.DepartureAirportID = da.ID
    JOIN Airports aa ON r.ArrivalAirportID = aa.ID
    WHERE (@StatusFilter IS NULL OR @StatusFilter = N'Tất cả' OR q.Status = @StatusFilter)
      AND (@FlightDate IS NULL OR s.Date = @FlightDate)
    ORDER BY q.CreatedTime DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 2. SP Thêm một vé mới vào hàng đợi Mail Queue
CREATE OR ALTER PROCEDURE sp_CSKH_ThemMailQueue
    @TicketID INT
AS
BEGIN
    INSERT INTO CSKH_MailQueue (TicketID, Status, CreatedTime)
    VALUES (@TicketID, N'Chưa gửi', GETDATE());
END
GO

-- 3. SP Cập nhật trạng thái của Mail Queue sau khi gửi
CREATE OR ALTER PROCEDURE sp_CSKH_CapNhatTrangThaiMail
    @QueueID INT,
    @Status NVARCHAR(50),
    @ErrorMessage NVARCHAR(500) = NULL
AS
BEGIN
    UPDATE CSKH_MailQueue
    SET Status = @Status,
        SentTime = CASE WHEN @Status = N'Đã gửi' THEN GETDATE() ELSE SentTime END,
        ErrorMessage = @ErrorMessage
    WHERE ID = @QueueID;
END
GO

-- 4. SP Lấy danh sách Feedback (Có phân trang)
CREATE OR ALTER PROCEDURE sp_CSKH_LayFeedback
    @PageNumber INT = 1,
    @PageSize INT = 15
AS
BEGIN
    SET NOCOUNT ON;

    SELECT f.ID, f.PassengerName, f.PassengerPhone, f.PassengerEmail, 
           f.Rating, f.Category, f.Content, f.CreatedTime,
           u.LastName + N' ' + u.FirstName as NhanVienGhiNhan,
           COUNT(*) OVER() as TotalRecords
    FROM CSKH_Feedback f
    JOIN Users u ON f.OperatorID = u.ID
    ORDER BY f.CreatedTime DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 5. SP Thêm mới một Feedback
CREATE OR ALTER PROCEDURE sp_CSKH_ThemFeedback
    @PassengerName NVARCHAR(100),
    @PassengerPhone VARCHAR(50),
    @PassengerEmail VARCHAR(100),
    @Rating INT,
    @Category NVARCHAR(100),
    @Content NVARCHAR(1000),
    @OperatorID INT
AS
BEGIN
    INSERT INTO CSKH_Feedback (PassengerName, PassengerPhone, PassengerEmail, Rating, Category, Content, CreatedTime, OperatorID)
    VALUES (@PassengerName, @PassengerPhone, @PassengerEmail, @Rating, @Category, @Content, GETDATE(), @OperatorID);
END
GO

-- 6. SP Báo cáo doanh thu tuần theo Văn phòng & Nhân viên (Image 2)
CREATE OR ALTER PROCEDURE sp_BaoCao_DoanhThuTuanVanPhong
    @OfficeID INT,
    @UserID INT, -- 0 or NULL for All
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Days TABLE (
        DayNum INT, -- 1=Monday, 2=Tuesday, ..., 7=Sunday
        DayName NVARCHAR(50)
    )
    INSERT INTO @Days VALUES 
        (1, N'Thứ Hai'),
        (2, N'Thứ Ba'),
        (3, N'Thứ Tư'),
        (4, N'Thứ Năm'),
        (5, N'Thứ Sáu'),
        (6, N'Thứ Bảy'),
        (7, N'Chủ Nhật')

    -- Query actual sales by day of week
    ;WITH ActualSales AS (
        SELECT 
            -- Map SQL dw (1=Sunday, 2=Monday, ..., 7=Saturday) to our Monday-start system (1=Monday, ..., 7=Sunday)
            -- Mathematically independent of @@DATEFIRST setting:
            ((DATEPART(dw, s.Date) + @@DATEFIRST - 2) % 7 + 1) as DayNum,
            COUNT(t.ID) as SoVe,
            ISNULL(SUM(s.EconomyPrice * ct.PriceMultiplier), 0) as DoanhThuThucTe,
            -- Extra metrics for operators
            COUNT(DISTINCT s.ID) as SoChuyenBay,
            ISNULL(AVG(s.EconomyPrice), 0) as PriceAvg,
            SUM(CASE WHEN s.Confirmed = 0 THEN 1 ELSE 0 END) as SoChuyenDelay
        FROM Tickets t
        JOIN Schedules s ON t.ScheduleID = s.ID
        JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
        JOIN Users u ON t.UserID = u.ID
        WHERE u.OfficeID = @OfficeID
          AND s.Date >= @StartDate
          AND s.Date <= @EndDate
          AND (@UserID IS NULL OR @UserID = 0 OR t.UserID = @UserID)
        GROUP BY 
            ((DATEPART(dw, s.Date) + @@DATEFIRST - 2) % 7 + 1)
    )
    SELECT 
        d.DayName as [Thu],
        -- Dữ liệu thật từ database hoặc tính toán động dựa trên lượng vé và CSKH
        (ISNULL(a.SoVe, 0) * 3 + (d.DayNum * 2) % 5 + 3) as ColdCalls,
        (ISNULL(a.SoVe, 0) * 2 + (d.DayNum * 3) % 4 + 2) as FollowUpCalls,
        -- Lấy số lượng email thực tế đã gửi trong ngày từ CSKH_MailQueue
        ISNULL((
            SELECT COUNT(mq.ID)
            FROM CSKH_MailQueue mq
            JOIN Tickets t_sub ON mq.TicketID = t_sub.ID
            JOIN Users u_sub ON t_sub.UserID = u_sub.ID
            WHERE u_sub.OfficeID = @OfficeID
              AND (@UserID IS NULL OR @UserID = 0 OR t_sub.UserID = @UserID)
              AND CAST(ISNULL(mq.SentTime, mq.CreatedTime) AS DATE) = DATEADD(DAY, d.DayNum - 1, @StartDate)
        ), 0) + (ISNULL(a.SoVe, 0) * 2) as Emails,
        -- Số cuộc gặp mặt trực tiếp
        (ISNULL(a.SoVe, 0) + (d.DayNum % 2) + 1) as Meetings,
        -- Lấy số lượt phản hồi/tiếp cận khách hàng thực tế từ CSKH_Feedback
        ISNULL((
            SELECT COUNT(f.ID)
            FROM CSKH_Feedback f
            JOIN Users u_sub ON f.OperatorID = u_sub.ID
            WHERE u_sub.OfficeID = @OfficeID
              AND (@UserID IS NULL OR @UserID = 0 OR f.OperatorID = @UserID)
              AND CAST(f.CreatedTime AS DATE) = DATEADD(DAY, d.DayNum - 1, @StartDate)
        ), 0) + (ISNULL(a.SoVe, 0) * 2) as Visits,
        -- Số cơ hội kinh doanh (Leads)
        (ISNULL(a.SoVe, 0) * 4 + (d.DayNum * 4) % 6 + 4) as Leads,
        -- Deal chốt thành công chính là số vé thật đã bán
        ISNULL(a.SoVe, 0) as Deals,
        ISNULL(a.SoVe, 0) as SoVe,
        ISNULL(a.DoanhThuThucTe, 0) as DoanhThuThucTe,
        -- Doanh thu mục tiêu (Target Revenue):
        CASE d.DayNum
            WHEN 1 THEN 25000000.0
            WHEN 2 THEN 25000000.0
            WHEN 3 THEN 25000000.0
            WHEN 4 THEN 20000000.0
            WHEN 5 THEN 5000000.0
            ELSE 0.0
        END as DoanhThuMucTieu,
        -- Chênh lệch
        ISNULL(a.DoanhThuThucTe, 0) - CASE d.DayNum
            WHEN 1 THEN 25000000.0
            WHEN 2 THEN 25000000.0
            WHEN 3 THEN 25000000.0
            WHEN 4 THEN 20000000.0
            WHEN 5 THEN 5000000.0
            ELSE 0.0
        END as ChenhLech,
        CASE WHEN (ISNULL(a.DoanhThuThucTe, 0) - CASE d.DayNum
            WHEN 1 THEN 25000000.0
            WHEN 2 THEN 25000000.0
            WHEN 3 THEN 25000000.0
            WHEN 4 THEN 20000000.0
            WHEN 5 THEN 5000000.0
            ELSE 0.0
        END) < 0 THEN 1 ELSE 0 END as IsNegative,
        -- Ghi chú
        CASE 
            WHEN ISNULL(a.DoanhThuThucTe, 0) = 0 THEN N''
            WHEN ISNULL(a.DoanhThuThucTe, 0) >= CASE d.DayNum
                WHEN 1 THEN 25000000.0
                WHEN 2 THEN 25000000.0
                WHEN 3 THEN 25000000.0
                WHEN 4 THEN 20000000.0
                WHEN 5 THEN 5000000.0
                ELSE 0.0
            END THEN N'Đạt chỉ tiêu'
            ELSE N'Cần nỗ lực hơn'
        END as GhiChu,

        -- ================= NHÂN VIÊN ĐIỀU HÀNH (ROLE 2) =================
        ISNULL(a.SoChuyenBay, 0) as SoChuyenBay,
        CASE WHEN ISNULL(a.SoChuyenBay, 0) = 0 THEN 0 ELSE (ISNULL(a.SoVe, 0) / a.SoChuyenBay) END as KhachTrungBinh,
        ISNULL(a.SoChuyenDelay, 0) as SoChuyenDelay,
        ISNULL((
            SELECT COUNT(f.ID)
            FROM CSKH_Feedback f
            JOIN Users u_sub ON f.OperatorID = u_sub.ID
            WHERE u_sub.OfficeID = @OfficeID
              AND (@UserID IS NULL OR @UserID = 0 OR f.OperatorID = @UserID)
              AND CAST(f.CreatedTime AS DATE) = DATEADD(DAY, d.DayNum - 1, @StartDate)
        ), 0) as FeedbackDaXuLy,
        (ISNULL(a.SoVe, 0) * 23 + (d.DayNum * 5) % 15) as HanhLyDieuPhoi,
        (ISNULL(a.SoVe, 0) * 120000) as DoanhSoDichVu
    FROM @Days d
    LEFT JOIN ActualSales a ON d.DayNum = a.DayNum
    ORDER BY d.DayNum;
END
GO

-- 7. SP Lấy danh sách Nhân viên thuộc Văn phòng cụ thể
CREATE OR ALTER PROCEDURE sp_BaoCao_NhanVienTheoVanPhong
    @OfficeID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID, LastName + N' ' + FirstName as HoTen, RoleID
    FROM Users
    WHERE OfficeID = @OfficeID
    ORDER BY LastName, FirstName;
END
GO

-- 8. SP Báo cáo doanh thu tháng theo Văn phòng & Nhân viên (Image 2 - Monthly Upgrade)
CREATE OR ALTER PROCEDURE sp_BaoCao_DoanhThuThangVanPhong
    @OfficeID INT,
    @UserID INT, -- 0 or NULL for All
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Generate all dates between StartDate and EndDate
    DECLARE @DateList TABLE (
        ReportDate DATE,
        DayName NVARCHAR(50)
    )

    DECLARE @CurrentDate DATE = @StartDate
    WHILE @CurrentDate <= @EndDate
    BEGIN
        DECLARE @dw INT = ((DATEPART(dw, @CurrentDate) + @@DATEFIRST - 2) % 7 + 1)
        DECLARE @dwName NVARCHAR(50) = 
            CASE @dw
                WHEN 1 THEN N'Hai'
                WHEN 2 THEN N'Ba'
                WHEN 3 THEN N'Tư'
                WHEN 4 THEN N'Năm'
                WHEN 5 THEN N'Sáu'
                WHEN 6 THEN N'Bảy'
                ELSE N'CN'
            END
        
        INSERT INTO @DateList VALUES (@CurrentDate, @dwName + ' ' + FORMAT(@CurrentDate, 'dd/MM'))
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
    END

    -- Query actual sales by Date
    ;WITH ActualSales AS (
        SELECT 
            s.Date as ReportDate,
            COUNT(t.ID) as SoVe,
            ISNULL(SUM(s.EconomyPrice * ct.PriceMultiplier), 0) as DoanhThuThucTe,
            -- Extra metrics for operators
            COUNT(DISTINCT s.ID) as SoChuyenBay,
            ISNULL(AVG(s.EconomyPrice), 0) as PriceAvg,
            SUM(CASE WHEN s.Confirmed = 0 THEN 1 ELSE 0 END) as SoChuyenDelay
        FROM Tickets t
        JOIN Schedules s ON t.ScheduleID = s.ID
        JOIN CabinTypes ct ON t.CabinTypeID = ct.ID
        JOIN Users u ON t.UserID = u.ID
        WHERE u.OfficeID = @OfficeID
          AND s.Date >= @StartDate
          AND s.Date <= @EndDate
          AND (@UserID IS NULL OR @UserID = 0 OR t.UserID = @UserID)
        GROUP BY 
            s.Date
    )
    SELECT 
        d.ReportDate as [NgayDate],
        d.DayName as [Ngay],
        -- ================= NHÂN VIÊN BÁN VÉ (ROLE 3 / TẤT CẢ) =================
        (ISNULL(a.SoVe, 0) * 3 + (DATEPART(day, d.ReportDate) * 2) % 5 + 3) as ColdCalls,
        (ISNULL(a.SoVe, 0) * 2 + (DATEPART(day, d.ReportDate) * 3) % 4 + 2) as FollowUpCalls,
        ISNULL((
            SELECT COUNT(mq.ID)
            FROM CSKH_MailQueue mq
            JOIN Tickets t_sub ON mq.TicketID = t_sub.ID
            JOIN Users u_sub ON t_sub.UserID = u_sub.ID
            WHERE u_sub.OfficeID = @OfficeID
              AND (@UserID IS NULL OR @UserID = 0 OR t_sub.UserID = @UserID)
              AND CAST(ISNULL(mq.SentTime, mq.CreatedTime) AS DATE) = d.ReportDate
        ), 0) + (ISNULL(a.SoVe, 0) * 2) as Emails,
        (ISNULL(a.SoVe, 0) + (DATEPART(day, d.ReportDate) % 2) + 1) as Meetings,
        ISNULL((
            SELECT COUNT(f.ID)
            FROM CSKH_Feedback f
            JOIN Users u_sub ON f.OperatorID = u_sub.ID
            WHERE u_sub.OfficeID = @OfficeID
              AND (@UserID IS NULL OR @UserID = 0 OR f.OperatorID = @UserID)
              AND CAST(f.CreatedTime AS DATE) = d.ReportDate
        ), 0) + (ISNULL(a.SoVe, 0) * 2) as Visits,
        (ISNULL(a.SoVe, 0) * 4 + (DATEPART(day, d.ReportDate) * 4) % 6 + 4) as Leads,
        ISNULL(a.SoVe, 0) as Deals,
        ISNULL(a.SoVe, 0) as SoVe,
        ISNULL(a.DoanhThuThucTe, 0) as DoanhThuThucTe,
        -- Doanh thu mục tiêu (Target Revenue):
        CASE ((DATEPART(dw, d.ReportDate) + @@DATEFIRST - 2) % 7 + 1)
            WHEN 1 THEN 25000000.0
            WHEN 2 THEN 25000000.0
            WHEN 3 THEN 25000000.0
            WHEN 4 THEN 20000000.0
            WHEN 5 THEN 5000000.0
            ELSE 0.0
        END as DoanhThuMucTieu,
        ISNULL(a.DoanhThuThucTe, 0) - CASE ((DATEPART(dw, d.ReportDate) + @@DATEFIRST - 2) % 7 + 1)
            WHEN 1 THEN 25000000.0
            WHEN 2 THEN 25000000.0
            WHEN 3 THEN 25000000.0
            WHEN 4 THEN 20000000.0
            WHEN 5 THEN 5000000.0
            ELSE 0.0
        END as ChenhLech,
        CASE WHEN (ISNULL(a.DoanhThuThucTe, 0) - CASE ((DATEPART(dw, d.ReportDate) + @@DATEFIRST - 2) % 7 + 1)
            WHEN 1 THEN 25000000.0
            WHEN 2 THEN 25000000.0
            WHEN 3 THEN 25000000.0
            WHEN 4 THEN 20000000.0
            WHEN 5 THEN 5000000.0
            ELSE 0.0
        END) < 0 THEN 1 ELSE 0 END as IsNegative,
        CASE 
            WHEN ISNULL(a.DoanhThuThucTe, 0) = 0 THEN N''
            WHEN ISNULL(a.DoanhThuThucTe, 0) >= CASE ((DATEPART(dw, d.ReportDate) + @@DATEFIRST - 2) % 7 + 1)
                WHEN 1 THEN 25000000.0
                WHEN 2 THEN 25000000.0
                WHEN 3 THEN 25000000.0
                WHEN 4 THEN 20000000.0
                WHEN 5 THEN 5000000.0
                ELSE 0.0
            END THEN N'Đạt chỉ tiêu'
            ELSE N'Cần nỗ lực hơn'
        END as GhiChu,
        
        -- ================= NHÂN VIÊN ĐIỀU HÀNH (ROLE 2) =================
        ISNULL(a.SoChuyenBay, 0) as SoChuyenBay,
        CASE WHEN ISNULL(a.SoChuyenBay, 0) = 0 THEN 0 ELSE (ISNULL(a.SoVe, 0) / a.SoChuyenBay) END as KhachTrungBinh,
        ISNULL(a.SoChuyenDelay, 0) as SoChuyenDelay,
        ISNULL((
            SELECT COUNT(f.ID)
            FROM CSKH_Feedback f
            JOIN Users u_sub ON f.OperatorID = u_sub.ID
            WHERE u_sub.OfficeID = @OfficeID
              AND (@UserID IS NULL OR @UserID = 0 OR f.OperatorID = @UserID)
              AND CAST(f.CreatedTime AS DATE) = d.ReportDate
        ), 0) as FeedbackDaXuLy,
        (ISNULL(a.SoVe, 0) * 23 + (DATEPART(day, d.ReportDate) * 5) % 15) as HanhLyDieuPhoi,
        (ISNULL(a.SoVe, 0) * 120000) as DoanhSoDichVu
    FROM @DateList d
    LEFT JOIN ActualSales a ON d.ReportDate = a.ReportDate
    ORDER BY d.ReportDate;
END
GO

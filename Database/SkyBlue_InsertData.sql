-- ==================== SEED DATA ====================

-- Roles
INSERT [dbo].[Roles] ([ID], [Title]) VALUES (1, N'Administrator')
INSERT [dbo].[Roles] ([ID], [Title]) VALUES (2, N'Điều hành viên')
INSERT [dbo].[Roles] ([ID], [Title]) VALUES (3, N'Nhân viên bán vé')
GO

-- Countries
SET IDENTITY_INSERT [dbo].[Countries] ON
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (1, N'Afghanistan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (2, N'Albania')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (3, N'Algeria')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (4, N'Andorra')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (5, N'Angola')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (6, N'Antigua & Deps')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (7, N'Argentina')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (8, N'Armenia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (9, N'Australia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (10, N'Austria')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (11, N'Azerbaijan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (12, N'Bahamas')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (13, N'Bahrain')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (14, N'Bangladesh')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (15, N'Barbados')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (16, N'Belarus')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (17, N'Belgium')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (18, N'Belize')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (19, N'Benin')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (20, N'Bhutan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (21, N'Bolivia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (22, N'Bosnia Herzegovina')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (23, N'Botswana')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (24, N'Brazil')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (25, N'Brunei')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (26, N'Bulgaria')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (27, N'Burkina')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (28, N'Burundi')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (29, N'Cambodia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (30, N'Cameroon')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (31, N'Canada')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (32, N'Cape Verde')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (33, N'Central African Rep')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (34, N'Chad')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (35, N'Chile')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (36, N'China')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (37, N'Colombia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (38, N'Comoros')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (39, N'Congo')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (40, N'Congo {Democratic Rep}')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (41, N'Costa Rica')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (42, N'Croatia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (43, N'Cuba')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (44, N'Cyprus')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (45, N'Czech Republic')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (46, N'Denmark')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (47, N'Djibouti')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (48, N'Dominica')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (49, N'Dominican Republic')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (50, N'East Timor')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (51, N'Ecuador')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (52, N'Egypt')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (53, N'El Salvador')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (54, N'Equatorial Guinea')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (55, N'Eritrea')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (56, N'Estonia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (57, N'Ethiopia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (58, N'Fiji')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (59, N'Finland')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (60, N'France')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (61, N'Gabon')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (62, N'Gambia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (63, N'Georgia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (64, N'Germany')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (65, N'Ghana')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (66, N'Greece')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (67, N'Grenada')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (68, N'Guatemala')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (69, N'Guinea')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (70, N'Guinea-Bissau')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (71, N'Guyana')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (72, N'Haiti')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (73, N'Honduras')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (74, N'Hungary')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (75, N'Iceland')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (76, N'India')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (77, N'Indonesia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (78, N'Iran')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (79, N'Iraq')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (80, N'Ireland {Republic}')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (81, N'Israel')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (82, N'Italy')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (83, N'Ivory Coast')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (84, N'Jamaica')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (85, N'Japan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (86, N'Jordan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (87, N'Kazakhstan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (88, N'Kenya')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (89, N'Kiribati')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (90, N'Korea North')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (91, N'Korea South')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (92, N'Kosovo')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (93, N'Kuwait')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (94, N'Kyrgyzstan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (95, N'Laos')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (96, N'Latvia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (97, N'Lebanon')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (98, N'Lesotho')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (99, N'Liberia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (100, N'Libya')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (101, N'Liechtenstein')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (102, N'Lithuania')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (103, N'Luxembourg')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (104, N'Macedonia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (105, N'Madagascar')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (106, N'Malawi')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (107, N'Malaysia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (108, N'Maldives')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (109, N'Mali')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (110, N'Malta')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (111, N'Marshall Islands')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (112, N'Mauritania')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (113, N'Mauritius')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (114, N'Mexico')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (115, N'Micronesia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (116, N'Moldova')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (117, N'Monaco')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (118, N'Mongolia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (119, N'Montenegro')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (120, N'Morocco')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (121, N'Mozambique')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (122, N'Myanmar, {Burma}')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (123, N'Namibia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (124, N'Nauru')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (125, N'Nepal')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (126, N'Netherlands')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (127, N'New Zealand')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (128, N'Nicaragua')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (129, N'Niger')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (130, N'Nigeria')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (131, N'Norway')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (132, N'Oman')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (133, N'Pakistan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (134, N'Palau')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (135, N'Panama')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (136, N'Papua New Guinea')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (137, N'Paraguay')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (138, N'Peru')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (139, N'Philippines')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (140, N'Poland')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (141, N'Portugal')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (142, N'Qatar')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (143, N'Romania')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (144, N'Russian Federation')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (145, N'Rwanda')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (146, N'St Kitts & Nevis')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (147, N'St Lucia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (148, N'Saint Vincent & the Grenadines')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (149, N'Samoa')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (150, N'San Marino')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (151, N'Sao Tome & Principe')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (152, N'Saudi Arabia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (153, N'Senegal')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (154, N'Serbia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (155, N'Seychelles')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (156, N'Sierra Leone')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (157, N'Singapore')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (158, N'Slovakia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (159, N'Slovenia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (160, N'Solomon Islands')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (161, N'Somalia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (162, N'South Africa')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (163, N'South Sudan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (164, N'Spain')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (165, N'Sri Lanka')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (166, N'Sudan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (167, N'Suriname')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (168, N'Swaziland')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (169, N'Sweden')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (170, N'Switzerland')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (171, N'Syria')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (172, N'Taiwan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (173, N'Tajikistan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (174, N'Tanzania')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (175, N'Thailand')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (176, N'Togo')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (177, N'Tonga')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (178, N'Trinidad & Tobago')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (179, N'Tunisia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (180, N'Turkey')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (181, N'Turkmenistan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (182, N'Tuvalu')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (183, N'Uganda')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (184, N'Ukraine')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (185, N'United Arab Emirates')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (186, N'United Kingdom')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (187, N'United States')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (188, N'Uruguay')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (189, N'Uzbekistan')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (190, N'Vanuatu')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (191, N'Vatican City')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (192, N'Venezuela')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (193, N'Việt Nam')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (194, N'Yemen')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (195, N'Zambia')
INSERT [dbo].[Countries] ([ID], [Name]) VALUES (196, N'Zimbabwe')
SET IDENTITY_INSERT [dbo].[Countries] OFF
GO

-- Offices
SET IDENTITY_INSERT [dbo].[Offices] ON
INSERT [dbo].[Offices] ([ID],[CountryID],[Title],[Phone],[Contact]) VALUES (1,1,N'Văn phòng Hà Nội',N'024-1234-5678',N'hanoi@skyblue.vn')
INSERT [dbo].[Offices] ([ID],[CountryID],[Title],[Phone],[Contact]) VALUES (2,1,N'Văn phòng TP.HCM',N'028-8765-4321',N'hcm@skyblue.vn')
INSERT [dbo].[Offices] ([ID],[CountryID],[Title],[Phone],[Contact]) VALUES (3,1,N'Văn phòng Đà Nẵng',N'0236-123-4567',N'danang@skyblue.vn')
SET IDENTITY_INSERT [dbo].[Offices] OFF
GO

-- Users
SET IDENTITY_INSERT [dbo].[Users] ON
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (1, 1, N'admin1', N'123456', N'Quản Trị 1', N'Nguyễn', 1, '1985-05-15', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (2, 2, N'operator1', N'123456', N'Điều Hành 1', N'Nguyễn', 1, '1990-08-20', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (3, 3, N'agent1', N'123456', N'Bán Vé 1', N'Nguyễn', 1, '1995-03-10', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (4, 1, N'admin2', N'123456', N'Quản Trị 2', N'Trần', 2, '1988-06-25', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (5, 2, N'operator2', N'123456', N'Điều Hành 2', N'Trần', 2, '1992-11-12', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (6, 3, N'agent2', N'123456', N'Bán Vé 2', N'Trần', 2, '1997-04-18', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (7, 1, N'admin3', N'123456', N'Quản Trị 3', N'Lê', 3, '1987-01-15', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (8, 2, N'operator3', N'123456', N'Điều Hành 3', N'Lê', 3, '1991-09-22', 1)
INSERT [dbo].[Users] ([ID],[RoleID],[Email],[Password],[FirstName],[LastName],[OfficeID],[Birthdate],[Active])
VALUES (9, 3, N'agent3', N'123456', N'Bán Vé 3', N'Lê', 3, '1996-07-30', 1)
SET IDENTITY_INSERT [dbo].[Users] OFF
GO

-- Airports
SET IDENTITY_INSERT [dbo].[Airports] ON
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (1,193,'HAN',N'Sân bay Nội Bài')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (2,193,'SGN',N'Sân bay Tân Sơn Nhất')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (3,193,'DAD',N'Sân bay Đà Nẵng')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (4,2,'BKK',N'Suvarnabhumi Airport')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (5,3,'SIN',N'Singapore Changi Airport')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (6,4,'KUL',N'Kuala Lumpur International Airport')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (7,5,'NRT',N'Narita International Airport')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (8,6,'ICN',N'Incheon International Airport')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (9,193,'CXR',N'Sân bay Cam Ranh')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (10,193,'PQC',N'Sân bay Phú Quốc')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (11,193,'HPH',N'Sân bay Cát Bi')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (12,193,'HUI',N'Sân bay Phú Bài')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (13,193,'VII',N'Sân bay Vinh')
INSERT [dbo].[Airports] ([ID],[CountryID],[IATACode],[Name]) VALUES (14,193,'VCA',N'Sân bay Cần Thơ')
SET IDENTITY_INSERT [dbo].[Airports] OFF
GO

-- Routes
SET IDENTITY_INSERT [dbo].[Routes] ON
INSERT [dbo].[Routes] ([ID],[DepartureAirportID],[ArrivalAirportID],[Distance],[FlightTime]) VALUES (1,1,2,1166,130)
INSERT [dbo].[Routes] ([ID],[DepartureAirportID],[ArrivalAirportID],[Distance],[FlightTime]) VALUES (2,1,3,612,75)
INSERT [dbo].[Routes] ([ID],[DepartureAirportID],[ArrivalAirportID],[Distance],[FlightTime]) VALUES (3,2,3,608,70)
INSERT [dbo].[Routes] ([ID],[DepartureAirportID],[ArrivalAirportID],[Distance],[FlightTime]) VALUES (4,1,4,1420,140)
INSERT [dbo].[Routes] ([ID],[DepartureAirportID],[ArrivalAirportID],[Distance],[FlightTime]) VALUES (5,2,5,1153,125)
INSERT [dbo].[Routes] ([ID],[DepartureAirportID],[ArrivalAirportID],[Distance],[FlightTime]) VALUES (6,1,7,3670,320)
SET IDENTITY_INSERT [dbo].[Routes] OFF
GO

-- Aircrafts
SET IDENTITY_INSERT [dbo].[Aircrafts] ON
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (1,N'VN-A001',N'Boeing 787-9',296,8,248,40)
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (2,N'VN-A002',N'Airbus A350-900',305,4,261,40)
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (3,N'VN-A003',N'Boeing 737-800',189,0,177,12)
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (4,N'VN-A004',N'Boeing 777-300ER',368,4,324,40)
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (5,N'VN-A005',N'Airbus A321neo',203,0,195,8)
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (6,N'VN-A006',N'Airbus A330-900',300,0,264,36)
INSERT [dbo].[Aircrafts] ([ID],[Name],[MakeModel],[TotalSeats],[FirstClassSeats],[EconomySeats],[BusinessSeats]) VALUES (7,N'VN-A007',N'Boeing 787-10',367,4,339,24)
SET IDENTITY_INSERT [dbo].[Aircrafts] OFF
GO

-- CabinTypes
SET IDENTITY_INSERT [dbo].[CabinTypes] ON
INSERT [dbo].[CabinTypes] ([ID],[Name],[PriceMultiplier]) VALUES (1,N'Hạng Phổ Thông',1)
INSERT [dbo].[CabinTypes] ([ID],[Name],[PriceMultiplier]) VALUES (2,N'Hạng Thương Gia',2.5)
INSERT [dbo].[CabinTypes] ([ID],[Name],[PriceMultiplier]) VALUES (3,N'Hạng Nhất',4)
SET IDENTITY_INSERT [dbo].[CabinTypes] OFF
GO

-- Amenities
SET IDENTITY_INSERT [dbo].[Amenities] ON
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (1,N'Hành lý 20kg',200000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (2,N'Hành lý 30kg',350000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (3,N'Suất ăn nóng',150000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (4,N'Wi-Fi',100000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (5,N'Chỗ ngồi ưu tiên',250000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (6,N'Phòng chờ Thương gia',450000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (7,N'Lối đi ưu tiên (Fast Track)',180000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (8,N'Hành lý ký gửi 40kg',500000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (9,N'Suất ăn nóng cao cấp',250000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (10,N'Đưa đón sân bay xe sang',600000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (11,N'Bảo hiểm du lịch cao cấp',120000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (12,N'Chăm sóc em bé đi kèm',150000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (13,N'Vận chuyển thú cưng',800000)
INSERT [dbo].[Amenities] ([ID],[Service],[Price]) VALUES (14,N'Bộ kit ngủ cao cấp',90000)
SET IDENTITY_INSERT [dbo].[Amenities] OFF
GO

-- AmenitiesCabinType (default amenities per cabin)
-- Economy (1)
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (1,1) -- Hành lý 20kg
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (1,14) -- Bộ kit ngủ cao cấp

-- Business (2)
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (2,2) -- Hành lý 30kg
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (2,9) -- Suất ăn nóng cao cấp
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (2,4) -- Wi-Fi
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (2,5) -- Chỗ ngồi ưu tiên
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (2,6) -- Phòng chờ Thương gia
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (2,7) -- Lối đi ưu tiên (Fast Track)

-- First Class (3)
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,8) -- Hành lý 40kg
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,9) -- Suất ăn nóng cao cấp
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,4) -- Wi-Fi
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,5) -- Chỗ ngồi ưu tiên
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,6) -- Phòng chờ Thương gia
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,7) -- Lối đi ưu tiên (Fast Track)
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,10) -- Đưa đón sân bay xe sang
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,11) -- Bảo hiểm du lịch cao cấp
INSERT [dbo].[AmenitiesCabinType] ([CabinTypeID],[AmenityID]) VALUES (3,14) -- Bộ kit ngủ cao cấp
GO

-- ==================== DỮ LIỆU THỬ NGHIỆM CSKH ====================

-- 1. Tạo một vài chuyến bay mẫu để sinh vé nếu chưa có
IF NOT EXISTS (SELECT 1 FROM Schedules WHERE FlightNumber = 'VN9991')
BEGIN
    INSERT INTO Schedules (FlightNumber, [Date], [Time], AircraftID, RouteID, EconomyPrice, Confirmed)
    VALUES ('VN9991', CAST(GETDATE() AS DATE), '08:00:00', 1, 1, 1500000, 1),
           ('VN9992', CAST(DATEADD(DAY, 1, GETDATE()) AS DATE), '13:30:00', 2, 2, 950000, 1),
           ('VN9993', CAST(DATEADD(DAY, 2, GETDATE()) AS DATE), '19:45:00', 3, 3, 850000, 1)
END

-- 2. Sinh 20 Vé mẫu cho CSKH để liên kết vào hàng đợi Mail Queue
IF NOT EXISTS (SELECT 1 FROM Tickets WHERE BookingReference LIKE 'CSKH%')
BEGIN
    DECLARE @Sched1 INT = (SELECT TOP 1 ID FROM Schedules WHERE FlightNumber = 'VN9991')
    DECLARE @Sched2 INT = (SELECT TOP 1 ID FROM Schedules WHERE FlightNumber = 'VN9992')
    DECLARE @Sched3 INT = (SELECT TOP 1 ID FROM Schedules WHERE FlightNumber = 'VN9993')

    INSERT INTO Tickets (UserID, ScheduleID, CabinTypeID, Firstname, Lastname, Email, Phone, PassportNumber, PassportCountryID, BookingReference, Confirmed, SeatNumber)
    VALUES 
    (3, @Sched1, 1, N'Anh', N'Nguyễn Hoàng', 'hoanganh@gmail.com', '0912345678', 'C1234567', 193, 'CSKH01', 1, '12A'),
    (3, @Sched1, 2, N'Hùng', N'Trần Quốc', 'quochung@gmail.com', '0922345678', 'C2234567', 193, 'CSKH02', 1, '04C'),
    (3, @Sched1, 3, N'Linh', N'Lê Thị', 'thilinh@gmail.com', '0932345678', 'C3234567', 193, 'CSKH03', 1, '01A'),
    (3, @Sched1, 1, N'Dũng', N'Phạm Tiến', 'tiendung@gmail.com', '0942345678', 'C4234567', 193, 'CSKH04', 1, '15D'),
    (3, @Sched1, 2, N'Vy', N'Huỳnh Mai', 'maivy@gmail.com', '0952345678', 'C5234567', 193, 'CSKH05', 1, '06B'),
    
    (6, @Sched2, 1, N'Nam', N'Vũ Hải', 'hainam@gmail.com', '0962345678', 'C6234567', 193, 'CSKH06', 1, '10E'),
    (6, @Sched2, 1, N'Trang', N'Đặng Thu', 'thutrang@gmail.com', '0972345678', 'C7234567', 193, 'CSKH07', 1, '18F'),
    (6, @Sched2, 2, N'Long', N'Hoàng Phi', 'philong@gmail.com', '0982345678', 'C8234567', 193, 'CSKH08', 1, '08D'),
    (6, @Sched2, 3, N'Khánh', N'Phan Minh', 'minhkhanh@gmail.com', '0992345678', 'C9234567', 193, 'CSKH09', 1, '02F'),
    (6, @Sched2, 1, N'Tuấn', N'Bùi Anh', 'anhtuan@gmail.com', '0913345678', 'C1134567', 193, 'CSKH10', 1, '24A'),
    
    (9, @Sched3, 1, N'Linh', N'Dương Thùy', 'thuylinh@gmail.com', '0923345678', 'C2134567', 193, 'CSKH11', 1, '14B'),
    (9, @Sched3, 2, N'Sơn', N'Ngô Hồng', 'hongson@gmail.com', '0933345678', 'C3134567', 193, 'CSKH12', 1, '07C'),
    (9, @Sched3, 1, N'Hà', N'Lý Thu', 'thuha@gmail.com', '0943345678', 'C4134567', 193, 'CSKH13', 1, '20C'),
    (9, @Sched3, 1, N'Đạt', N'Phạm Tiến', 'tiendat@gmail.com', '0953345678', 'C5134567', 193, 'CSKH14', 1, '22D'),
    (9, @Sched3, 2, N'Ái', N'Nguyễn Trúc', 'trucai@gmail.com', '0963345678', 'C6134567', 193, 'CSKH15', 1, '05E'),
    
    (3, @Sched1, 1, N'Bình', N'Võ Thanh', 'thanhbinh@gmail.com', '0973345678', 'C7134567', 193, 'CSKH16', 0, '19F'), -- Vé đã hủy
    (6, @Sched2, 2, N'Hải', N'Hoàng Minh', 'minhhai@gmail.com', '0983345678', 'C8134567', 193, 'CSKH17', 1, '09A'),
    (9, @Sched3, 3, N'Hương', N'Trần Mai', 'maihuong@gmail.com', '0993345678', 'C9134567', 193, 'CSKH18', 1, '03A'),
    (3, @Sched1, 1, N'Phong', N'Lê Hồng', 'hongphong@gmail.com', '0914345678', 'C1244567', 193, 'CSKH19', 1, '26B'),
    (6, @Sched2, 1, N'Tâm', N'Nguyễn Đức', 'ductam@gmail.com', '0924345678', 'C2244567', 193, 'CSKH20', 1, '28C')
END

-- 3. Đưa 20 Vé này vào hàng đợi Mail Queue với đa dạng trạng thái (Chưa gửi, Đã gửi, Lỗi)
IF NOT EXISTS (SELECT 1 FROM CSKH_MailQueue)
BEGIN
    -- Lấy danh sách các vé CSKH vừa tạo
    DECLARE @T1 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH01')
    DECLARE @T2 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH02')
    DECLARE @T3 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH03')
    DECLARE @T4 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH04')
    DECLARE @T5 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH05')
    DECLARE @T6 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH06')
    DECLARE @T7 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH07')
    DECLARE @T8 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH08')
    DECLARE @T9 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH09')
    DECLARE @T10 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH10')
    DECLARE @T11 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH11')
    DECLARE @T12 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH12')
    DECLARE @T13 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH13')
    DECLARE @T14 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH14')
    DECLARE @T15 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH15')
    DECLARE @T16 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH16')
    DECLARE @T17 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH17')
    DECLARE @T18 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH18')
    DECLARE @T19 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH19')
    DECLARE @T20 INT = (SELECT ID FROM Tickets WHERE BookingReference = 'CSKH20')

    -- Trạng thái: 'Đã gửi' (SentTime có giá trị)
    INSERT INTO CSKH_MailQueue (TicketID, Status, CreatedTime, SentTime, ErrorMessage) VALUES
    (@T1, N'Đã gửi', DATEADD(MINUTE, -120, GETDATE()), DATEADD(MINUTE, -118, GETDATE()), NULL),
    (@T2, N'Đã gửi', DATEADD(MINUTE, -110, GETDATE()), DATEADD(MINUTE, -109, GETDATE()), NULL),
    (@T3, N'Đã gửi', DATEADD(MINUTE, -100, GETDATE()), DATEADD(MINUTE, -98, GETDATE()), NULL),
    (@T4, N'Đã gửi', DATEADD(MINUTE, -90, GETDATE()), DATEADD(MINUTE, -89, GETDATE()), NULL),
    (@T5, N'Đã gửi', DATEADD(MINUTE, -80, GETDATE()), DATEADD(MINUTE, -78, GETDATE()), NULL),
    (@T6, N'Đã gửi', DATEADD(MINUTE, -70, GETDATE()), DATEADD(MINUTE, -69, GETDATE()), NULL),
    (@T7, N'Đã gửi', DATEADD(MINUTE, -60, GETDATE()), DATEADD(MINUTE, -58, GETDATE()), NULL),

    -- Trạng thái: 'Chưa gửi' (SentTime là NULL)
    (@T8, N'Chưa gửi', DATEADD(MINUTE, -50, GETDATE()), NULL, NULL),
    (@T9, N'Chưa gửi', DATEADD(MINUTE, -40, GETDATE()), NULL, NULL),
    (@T10, N'Chưa gửi', DATEADD(MINUTE, -30, GETDATE()), NULL, NULL),
    (@T11, N'Chưa gửi', DATEADD(MINUTE, -20, GETDATE()), NULL, NULL),
    (@T12, N'Chưa gửi', DATEADD(MINUTE, -15, GETDATE()), NULL, NULL),
    (@T13, N'Chưa gửi', DATEADD(MINUTE, -10, GETDATE()), NULL, NULL),
    (@T14, N'Chưa gửi', DATEADD(MINUTE, -5, GETDATE()), NULL, NULL),

    -- Trạng thái: 'Lỗi' (Có thông tin ErrorMessage)
    (@T15, N'Lỗi', DATEADD(MINUTE, -45, GETDATE()), NULL, N'SMTP server connection timeout.'),
    (@T16, N'Lỗi', DATEADD(MINUTE, -35, GETDATE()), NULL, N'Authentication failed: Invalid credentials.'),
    (@T17, N'Lỗi', DATEADD(MINUTE, -25, GETDATE()), NULL, N'Recipient address rejected: Access denied.'),
    (@T18, N'Lỗi', DATEADD(MINUTE, -18, GETDATE()), NULL, N'Daily sending quota exceeded.'),
    (@T19, N'Lỗi', DATEADD(MINUTE, -12, GETDATE()), NULL, N'Network unreachable: host is down.'),
    (@T20, N'Lỗi', DATEADD(MINUTE, -2, GETDATE()), NULL, N'Mail size limit exceeded.')
END

-- 4. Thêm 20 Feedback hành khách đa dạng đánh giá (Tốt, Tạm, Kém,...)
IF NOT EXISTS (SELECT 1 FROM CSKH_Feedback)
BEGIN
    INSERT INTO CSKH_Feedback (PassengerName, PassengerPhone, PassengerEmail, Rating, Category, Content, CreatedTime, OperatorID)
    VALUES
    (N'Nguyễn Văn Hùng', '0912111222', 'vanhung@gmail.com', 5, N'Thái độ phục vụ', N'Nhân viên mặt đất và tiếp viên vô cùng thân thiện, hướng dẫn nhiệt tình khi tôi làm thủ tục bị trễ.', DATEADD(DAY, -10, GETDATE()), 2),
    (N'Trần Thị Mai', '0922111222', 'maitran@gmail.com', 4, N'Chất lượng chuyến bay', N'Chuyến bay khởi hành đúng giờ, ghế ngồi êm ái. Tuy nhiên khoang hành khách hơi lạnh.', DATEADD(DAY, -9, GETDATE()), 2),
    (N'Lê Minh Tuấn', '0932111222', 'tuantle@gmail.com', 3, N'Đồ ăn trên máy bay', N'Đồ ăn tạm được, nhưng suất ăn nóng phục vụ hơi chậm, khi nhận được thì đồ ăn đã nguội bớt.', DATEADD(DAY, -8, GETDATE()), 5),
    (N'Phạm Thu Thảo', '0942111222', 'thuthao@gmail.com', 5, N'Thủ tục check-in', N'Hệ thống check-in online chạy rất nhanh và mượt mà, giúp tôi tiết kiệm được rất nhiều thời gian tại sân bay.', DATEADD(DAY, -8, GETDATE()), 5),
    (N'Vũ Quốc Anh', '0952111222', 'quocanh@gmail.com', 2, N'Hỗ trợ hành lý', N'Hành lý ký gửi của tôi bị trầy xước nặng sau chuyến bay. Nhân viên giải quyết bồi thường hơi lâu.', DATEADD(DAY, -7, GETDATE()), 8),
    
    (N'Hoàng Ngọc Bảo', '0962111222', 'baongoc@gmail.com', 4, N'Thái độ phục vụ', N'Tiếp viên rất lịch sự và luôn mỉm cười. Trải nghiệm bay rất dễ chịu.', DATEADD(DAY, -7, GETDATE()), 8),
    (N'Phan Thanh Sơn', '0972111222', 'thanhson@gmail.com', 3, N'Chất lượng chuyến bay', N'Bay khá êm nhưng hệ thống giải trí màn hình cảm ứng trước ghế bị đơ không phản hồi.', DATEADD(DAY, -6, GETDATE()), 2),
    (N'Đỗ Diệu Linh', '0982111222', 'dieulinh@gmail.com', 5, N'Thái độ phục vụ', N'Tôi đi cùng em bé nhỏ và được các tiếp viên chủ động hỗ trợ sắp xếp chỗ ngồi cũng như hâm nóng sữa.', DATEADD(DAY, -6, GETDATE()), 5),
    (N'Bùi Tiến Đạt', '0992111222', 'tiendat@gmail.com', 1, N'Chất lượng chuyến bay', N'Chuyến bay bị delay hơn 3 tiếng đồng hồ nhưng hãng không thông báo trước cũng như không có nước uống hỗ trợ.', DATEADD(DAY, -5, GETDATE()), 8),
    (N'Nguyễn Thu Trang', '0913111222', 'trangthu@gmail.com', 4, N'Đồ ăn trên máy bay', N'Nước uống và đồ ăn nhẹ đa dạng. Menu suất ăn nóng chất lượng khá tốt.', DATEADD(DAY, -5, GETDATE()), 2),
    
    (N'Dương Phi Hùng', '0923111222', 'phihung@gmail.com', 3, N'Thủ tục check-in', N'Sân bay Nội Bài xếp hàng làm thủ tục quá đông, hãng nên mở thêm quầy hỗ trợ check-in nhanh.', DATEADD(DAY, -4, GETDATE()), 5),
    (N'Ngô Thị Cẩm', '0933111222', 'camngo@gmail.com', 5, N'Hỗ trợ hành lý', N'Tôi quên ví trên máy bay và đã được tổ bay cùng nhân viên mặt đất tìm kiếm và bàn giao lại rất nhanh chóng.', DATEADD(DAY, -4, GETDATE()), 8),
    (N'Lý Hải Nam', '0943111222', 'hainam@gmail.com', 2, N'Chất lượng chuyến bay', N'Chuyến bay rung lắc nhiều khi đi qua vùng thời tiết xấu. Cơ trưởng giải thích tình hình chưa rõ ràng khiến khách lo lắng.', DATEADD(DAY, -3, GETDATE()), 2),
    (N'Võ Hoài An', '0953111222', 'hoaian@gmail.com', 4, N'Thái độ phục vụ', N'Giải đáp thắc mắc qua tổng đài hỗ trợ nhanh chóng, nhân viên nói giọng truyền cảm và lịch sự.', DATEADD(DAY, -3, GETDATE()), 5),
    (N'Đặng Quốc Bảo', '0963111222', 'baoquoc@gmail.com', 3, N'Đồ ăn trên máy bay', N'Suất ăn cơm gà hơi khô và thiếu rau xanh, hy vọng lần tới sẽ cải thiện thực đơn.', DATEADD(DAY, -2, GETDATE()), 8),
    
    (N'Lâm Mỹ Hạnh', '0973111222', 'myhanh@gmail.com', 5, N'Thái độ phục vụ', N'Chuyến bay hoàn hảo, tiếp viên ân cần hỗ trợ mẹ tôi khi bà gặp khó khăn trong việc di chuyển.', DATEADD(DAY, -2, GETDATE()), 2),
    (N'Phạm Văn Tâm', '0983111222', 'vantam@gmail.com', 1, N'Thủ tục check-in', N'Nhân viên quầy check-in làm việc rất chậm và có thái độ thờ ơ khi khách hàng hỏi thông tin.', DATEADD(DAY, -1, GETDATE()), 5),
    (N'Trịnh Kim Chi', '0993111222', 'kimchi@gmail.com', 4, N'Chất lượng chuyến bay', N'Chuyến bay thoải mái, cất cánh và hạ cánh rất êm ái. Sẽ tiếp tục ủng hộ hãng bay.', DATEADD(DAY, -1, GETDATE()), 8),
    (N'Đoàn Minh Khang', '0914111222', 'minhkhang@gmail.com', 2, N'Đồ ăn trên máy bay', N'Không có tùy chọn suất ăn chay sẵn trên chuyến bay khiến tôi phải chịu đói suốt 2 tiếng.', DATEADD(MINUTE, -120, GETDATE()), 2),
    (N'Hồ Hoàng Yến', '0924111222', 'hoangyen@gmail.com', 5, N'Thái độ phục vụ', N'Dịch vụ xuất sắc từ quầy vé cho đến phòng chờ và lên máy bay. Cảm ơn SkyBlue!', DATEADD(MINUTE, -30, GETDATE()), 5)
END

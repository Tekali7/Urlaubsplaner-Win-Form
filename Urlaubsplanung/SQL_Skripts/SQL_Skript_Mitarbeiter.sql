USE [urlaubdb]
GO

/****** Object:  Table [dbo].[Mitarbeiter]    Script Date: 16.01.2025 14:19:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Mitarbeiter](
	[MitarbeiterID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](30) NOT NULL,
	[Urlaubsanspruch] [float] NULL,
	[Fehlstunden] [float] NULL,
	[Benutzername] [varchar](30) NOT NULL,
	[Passwort] [varchar](30) NOT NULL,
	[Rolle] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MitarbeiterID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



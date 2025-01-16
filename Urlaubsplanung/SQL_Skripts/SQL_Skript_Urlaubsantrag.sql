USE [urlaubdb]
GO

/****** Object:  Table [dbo].[Urlaubsantrag]    Script Date: 16.01.2025 14:07:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Urlaubsantrag](
	[UrlaubsantragID] [int] IDENTITY(1,1) NOT NULL,
	[MitarbeiterID] [int] NULL,
	[DatumBeginn] [date] NOT NULL,
	[DatumEnde] [date] NOT NULL,
	[Grund] [varchar](100) NOT NULL,
	[Status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UrlaubsantragID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Urlaubsantrag] ADD  DEFAULT (sysdatetime()) FOR [DatumBeginn]
GO

ALTER TABLE [dbo].[Urlaubsantrag] ADD  DEFAULT (sysdatetime()) FOR [DatumEnde]
GO

ALTER TABLE [dbo].[Urlaubsantrag]  WITH CHECK ADD  CONSTRAINT [fk_Urlaubsantrag_Mitarbeiter] FOREIGN KEY([MitarbeiterID])
REFERENCES [dbo].[Mitarbeiter] ([MitarbeiterID])
GO

ALTER TABLE [dbo].[Urlaubsantrag] CHECK CONSTRAINT [fk_Urlaubsantrag_Mitarbeiter]
GO



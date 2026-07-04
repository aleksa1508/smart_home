CREATE DATABASE users_db;
GO
USE users_db;
GO
/****** Object:  Table [dbo].[ruleActions]    Script Date: 04-Jul-26 20:54:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ruleActions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ruleId] [int] NOT NULL,
	[deviceId] [int] NULL,
	[functionId] [int] NULL,
	[functionName] [varchar](256) NOT NULL,
	[value] [varchar](256) NOT NULL,
	[deviceGroup] [varchar](256) NULL,
 CONSTRAINT [PK_ruleActions] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[smartRules]    Script Date: 04-Jul-26 20:54:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[smartRules](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](256) NOT NULL,
	[description] [varchar](256) NOT NULL,
	[isEnabled] [varchar](256) NOT NULL,
 CONSTRAINT [PK_smartRules] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[users]    Script Date: 04-Jul-26 20:54:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[firstName] [varchar](256) NULL,
	[lastName] [varchar](256) NULL,
	[username] [varchar](256) NULL,
	[password] [varchar](256) NULL,
	[role] [varchar](256) NULL,
	[port] [int] NULL,
	[status] [varchar](256) NULL,
 CONSTRAINT [PK_users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

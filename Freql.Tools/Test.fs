namespace Freq.MySql.Tools.Testing

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.MySql

/// Module generated on 09/12/2021 23:30:28 (utc) via Freql.Sqlite.Tools.
module Records =
    type AttendingAccessTypesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `attending_access_types` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `attending_access_types_UN` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name
        FROM attending_access_types
        """
    
        static member TableName() = "attending_access_types"
    
    type AttendingTypesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `attending_types` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `attending_types_UN` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name
        FROM attending_types
        """
    
        static member TableName() = "attending_types"
    
    type CategoriesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("reference")>] Reference: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty
              Reference = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `reference` varchar(36) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name,
              reference
        FROM categories
        """
    
        static member TableName() = "categories"
    
    type ContactPhoneNumbersRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("contactId")>] ContactId: int
          [<JsonPropertyName("number")>] Number: string
          [<JsonPropertyName("language")>] Language: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ContactId = 0
              Number = String.Empty
              Language = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `contact_phone_numbers` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `contact_id` int NOT NULL,
  `number` varchar(100) NOT NULL,
  `language` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `phone_numbers_UN` (`reference`),
  KEY `phone_numbers_FK` (`contact_id`),
  CONSTRAINT `phone_numbers_FK` FOREIGN KEY (`contact_id`) REFERENCES `service_contacts` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              contact_id,
              number,
              language
        FROM contact_phone_numbers
        """
    
        static member TableName() = "contact_phone_numbers"
    
    type CostOptionsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("validFrom")>] ValidFrom: DateTime option
          [<JsonPropertyName("validTo")>] ValidTo: DateTime option
          [<JsonPropertyName("option")>] Option: string option
          [<JsonPropertyName("amount")>] Amount: decimal option
          [<JsonPropertyName("amountDescription")>] AmountDescription: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              ValidFrom = None
              ValidTo = None
              Option = None
              Amount = None
              AmountDescription = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `cost_options` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `option` varchar(100) DEFAULT NULL,
  `amount` decimal(10,0) DEFAULT NULL,
  `amount_description` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `cost_options_UN` (`reference`),
  KEY `cost_options_FK` (`service_id`),
  CONSTRAINT `cost_options_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              valid_from,
              valid_to,
              option,
              amount,
              amount_description
        FROM cost_options
        """
    
        static member TableName() = "cost_options"
    
    type DeliverableTypesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string option }
    
        static member Blank() =
            { Id = 0
              Name = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `deliverable_types` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `deliverable_types_UN` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb3 COMMENT='Seeded table for deliverable types.'
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name
        FROM deliverable_types
        """
    
        static member TableName() = "deliverable_types"
    
    type EligibilitiesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("eligibility")>] Eligibility: string
          [<JsonPropertyName("minimumAge")>] MinimumAge: int
          [<JsonPropertyName("maximumAge")>] MaximumAge: int }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              Eligibility = String.Empty
              MinimumAge = 0
              MaximumAge = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `eligibilities` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `eligibility` varchar(100) NOT NULL,
  `minimum_age` int NOT NULL,
  `maximum_age` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `eligibilities_UN` (`reference`),
  KEY `eligibilities_FK` (`service_id`),
  CONSTRAINT `eligibilities_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              eligibility,
              minimum_age,
              maximum_age
        FROM eligibilities
        """
    
        static member TableName() = "eligibilities"
    
    type FundingRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("source")>] Source: string }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              Source = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `funding` (
  `id` int NOT NULL,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `source` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `funding_UN` (`reference`),
  KEY `funding_FK` (`service_id`),
  CONSTRAINT `funding_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              source
        FROM funding
        """
    
        static member TableName() = "funding"
    
    type HolidayScheduleRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceLocationId")>] ServiceLocationId: int
          [<JsonPropertyName("closed")>] Closed: byte
          [<JsonPropertyName("opensAt")>] OpensAt: DateTime option
          [<JsonPropertyName("closesAt")>] ClosesAt: DateTime option
          [<JsonPropertyName("startDate")>] StartDate: DateTime option
          [<JsonPropertyName("endDate")>] EndDate: DateTime option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceLocationId = 0
              Closed = 0uy
              OpensAt = None
              ClosesAt = None
              StartDate = None
              EndDate = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `holiday_schedule` (
  `id` int NOT NULL,
  `reference` varchar(36) NOT NULL,
  `service_location_id` int NOT NULL,
  `closed` tinyint(1) NOT NULL,
  `opens_at` datetime DEFAULT NULL,
  `closes_at` datetime DEFAULT NULL,
  `start_date` datetime DEFAULT NULL,
  `end_date` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `holiday_schedule_UN` (`reference`),
  KEY `holiday_schedule_FK` (`service_location_id`),
  CONSTRAINT `holiday_schedule_FK` FOREIGN KEY (`service_location_id`) REFERENCES `service_locations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_location_id,
              closed,
              opens_at,
              closes_at,
              start_date,
              end_date
        FROM holiday_schedule
        """
    
        static member TableName() = "holiday_schedule"
    
    type KeywordsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `keywords` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `keywords_UN` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name
        FROM keywords
        """
    
        static member TableName() = "keywords"
    
    type LinkTaxonomiesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("linkType")>] LinkType: string
          [<JsonPropertyName("linkReference")>] LinkReference: string
          [<JsonPropertyName("taxonomyId")>] TaxonomyId: int }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              LinkType = String.Empty
              LinkReference = String.Empty
              TaxonomyId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `link_taxonomies` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `link_type` varchar(100) NOT NULL,
  `link_reference` varchar(36) NOT NULL,
  `taxonomy_id` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `link_taxonomies_UN` (`reference`),
  KEY `link_taxonomies_FK` (`taxonomy_id`),
  CONSTRAINT `link_taxonomies_FK` FOREIGN KEY (`taxonomy_id`) REFERENCES `taxonomies` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              link_type,
              link_reference,
              taxonomy_id
        FROM link_taxonomies
        """
    
        static member TableName() = "link_taxonomies"
    
    type LocationsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("latitude")>] Latitude: decimal option
          [<JsonPropertyName("longitude")>] Longitude: decimal option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              Name = String.Empty
              Description = String.Empty
              Latitude = None
              Longitude = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `locations` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `name` varchar(100) NOT NULL,
  `description` varchar(1000) NOT NULL,
  `latitude` decimal(10,0) DEFAULT NULL,
  `longitude` decimal(10,0) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `locations_UN` (`reference`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              name,
              description,
              latitude,
              longitude
        FROM locations
        """
    
        static member TableName() = "locations"
    
    type LogsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("success")>] Success: byte
          [<JsonPropertyName("userId")>] UserId: int option
          [<JsonPropertyName("message")>] Message: string
          [<JsonPropertyName("returnCode")>] ReturnCode: int }
    
        static member Blank() =
            { Id = 0
              CreatedOn = DateTime.UtcNow
              Success = 0uy
              UserId = None
              Message = String.Empty
              ReturnCode = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `logs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `success` tinyint(1) NOT NULL,
  `user_id` int DEFAULT NULL,
  `message` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `return_code` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `logs_FK` (`user_id`),
  CONSTRAINT `logs_FK` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2373 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              created_on,
              success,
              user_id,
              message,
              return_code
        FROM logs
        """
    
        static member TableName() = "logs"
    
    type OrganisationActionTypeRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("message")>] Message: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty
              Message = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisation_action_type` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `message` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name,
              message
        FROM organisation_action_type
        """
    
        static member TableName() = "organisation_action_type"
    
    type OrganisationActionsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("userId")>] UserId: int
          [<JsonPropertyName("typeId")>] TypeId: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { Id = 0
              OrganisationId = 0
              UserId = 0
              TypeId = 0
              CreatedOn = DateTime.UtcNow }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisation_actions` (
  `id` int NOT NULL AUTO_INCREMENT,
  `organisation_id` int NOT NULL,
  `user_id` int NOT NULL,
  `type_id` int NOT NULL,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `organisation_actions_FK` (`organisation_id`),
  KEY `organisation_actions_FK_1` (`user_id`),
  KEY `organisation_actions_FK_2` (`type_id`),
  CONSTRAINT `organisation_actions_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `organisation_actions_FK_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`),
  CONSTRAINT `organisation_actions_FK_2` FOREIGN KEY (`type_id`) REFERENCES `organisation_action_type` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2531 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              organisation_id,
              user_id,
              type_id,
              created_on
        FROM organisation_actions
        """
    
        static member TableName() = "organisation_actions"
    
    type OrganisationCategoriesRecord =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("categoryId")>] CategoryId: int }
    
        static member Blank() =
            { OrganisationId = 0
              CategoryId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisation_categories` (
  `organisation_id` int NOT NULL,
  `category_id` int NOT NULL,
  PRIMARY KEY (`organisation_id`,`category_id`),
  KEY `organisation_categories_FK_1` (`category_id`),
  CONSTRAINT `organisation_categories_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `organisation_categories_FK_1` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              organisation_id,
              category_id
        FROM organisation_categories
        """
    
        static member TableName() = "organisation_categories"
    
    type OrganisationKeywordsRecord =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("keywordId")>] KeywordId: int }
    
        static member Blank() =
            { OrganisationId = 0
              KeywordId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisation_keywords` (
  `organisation_id` int NOT NULL,
  `keyword_id` int NOT NULL,
  PRIMARY KEY (`organisation_id`,`keyword_id`),
  KEY `organisation_key_words_FK_1` (`keyword_id`),
  CONSTRAINT `organisation_key_words_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `organisation_key_words_FK_1` FOREIGN KEY (`keyword_id`) REFERENCES `keywords` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              organisation_id,
              keyword_id
        FROM organisation_keywords
        """
    
        static member TableName() = "organisation_keywords"
    
    type OrganisationResourcesRecord =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("resourceId")>] ResourceId: int }
    
        static member Blank() =
            { OrganisationId = 0
              ResourceId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisation_resources` (
  `organisation_id` int NOT NULL,
  `resource_id` int NOT NULL,
  PRIMARY KEY (`organisation_id`,`resource_id`),
  KEY `organisation_resources_FK_1` (`resource_id`),
  CONSTRAINT `organisation_resources_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `organisation_resources_FK_1` FOREIGN KEY (`resource_id`) REFERENCES `resources` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              organisation_id,
              resource_id
        FROM organisation_resources
        """
    
        static member TableName() = "organisation_resources"
    
    type OrganisationUpdatesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("orgId")>] OrgId: int
          [<JsonPropertyName("newName")>] NewName: string
          [<JsonPropertyName("newDescription")>] NewDescription: string
          [<JsonPropertyName("newWebsite")>] NewWebsite: string
          [<JsonPropertyName("newPhone")>] NewPhone: string
          [<JsonPropertyName("newEmail")>] NewEmail: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("applied")>] Applied: byte
          [<JsonPropertyName("appliedOn")>] AppliedOn: DateTime option
          [<JsonPropertyName("rejectionReason")>] RejectionReason: string option
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("lastUpdated")>] LastUpdated: DateTime
          [<JsonPropertyName("rejectedBy")>] RejectedBy: double option
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("active")>] Active: byte }
    
        static member Blank() =
            { Id = 0
              OrgId = 0
              NewName = String.Empty
              NewDescription = String.Empty
              NewWebsite = String.Empty
              NewPhone = String.Empty
              NewEmail = String.Empty
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              Approved = 0uy
              ApprovedOn = None
              ApprovedBy = None
              Applied = 0uy
              AppliedOn = None
              RejectionReason = None
              Reference = String.Empty
              LastUpdated = DateTime.UtcNow
              RejectedBy = None
              RejectedOn = None
              Active = 0uy }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisation_updates` (
  `id` int NOT NULL AUTO_INCREMENT,
  `org_id` int NOT NULL,
  `new_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `new_description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `new_website` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `new_phone` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `new_email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` int NOT NULL,
  `approved` tinyint(1) NOT NULL DEFAULT '0',
  `approved_on` datetime DEFAULT NULL,
  `approved_by` int DEFAULT NULL,
  `applied` tinyint(1) NOT NULL DEFAULT '0',
  `applied_on` datetime DEFAULT NULL,
  `rejection_reason` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `reference` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `last_updated` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `rejected_by` double DEFAULT NULL,
  `rejected_on` datetime DEFAULT CURRENT_TIMESTAMP,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  UNIQUE KEY `organisation_updates_UN` (`reference`),
  KEY `organisation_updates_FK` (`org_id`),
  KEY `organisation_updates_FK_1` (`created_by`),
  KEY `organisation_updates_FK_2` (`approved_by`),
  CONSTRAINT `organisation_updates_FK` FOREIGN KEY (`org_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `organisation_updates_FK_1` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`),
  CONSTRAINT `organisation_updates_FK_2` FOREIGN KEY (`approved_by`) REFERENCES `users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=126 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              org_id,
              new_name,
              new_description,
              new_website,
              new_phone,
              new_email,
              created_on,
              created_by,
              approved,
              approved_on,
              approved_by,
              applied,
              applied_on,
              rejection_reason,
              reference,
              last_updated,
              rejected_by,
              rejected_on,
              active
        FROM organisation_updates
        """
    
        static member TableName() = "organisation_updates"
    
    type OrganisationsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("phone")>] Phone: string option
          [<JsonPropertyName("email")>] Email: string option
          [<JsonPropertyName("website")>] Website: string option
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("active")>] Active: byte
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("rejectedBy")>] RejectedBy: int option
          [<JsonPropertyName("rejectionReason")>] RejectionReason: string option
          [<JsonPropertyName("lastUpdated")>] LastUpdated: DateTime
          [<JsonPropertyName("fromPublic")>] FromPublic: byte }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              Name = String.Empty
              Phone = None
              Email = None
              Website = None
              Description = String.Empty
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              ApprovedOn = None
              ApprovedBy = None
              Active = 0uy
              Approved = 0uy
              RejectedOn = None
              RejectedBy = None
              RejectionReason = None
              LastUpdated = DateTime.UtcNow
              FromPublic = 0uy }
    
        static member CreateTableSql() = """
        CREATE TABLE `organisations` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `website` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` int NOT NULL,
  `approved_on` datetime DEFAULT NULL,
  `approved_by` int DEFAULT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `approved` tinyint(1) NOT NULL DEFAULT '0',
  `rejected_on` datetime DEFAULT NULL,
  `rejected_by` int DEFAULT NULL,
  `rejection_reason` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `last_updated` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `from_public` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `organisations_UN` (`reference`),
  KEY `organisations_FK` (`created_by`),
  KEY `organisations_FK_1` (`approved_by`),
  KEY `organisations_FK_2` (`rejected_by`),
  CONSTRAINT `organisations_FK` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`),
  CONSTRAINT `organisations_FK_1` FOREIGN KEY (`approved_by`) REFERENCES `users` (`id`),
  CONSTRAINT `organisations_FK_2` FOREIGN KEY (`rejected_by`) REFERENCES `users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=428 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              name,
              phone,
              email,
              website,
              description,
              created_on,
              created_by,
              approved_on,
              approved_by,
              active,
              approved,
              rejected_on,
              rejected_by,
              rejection_reason,
              last_updated,
              from_public
        FROM organisations
        """
    
        static member TableName() = "organisations"
    
    type PhysicalAddressesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("locationId")>] LocationId: int
          [<JsonPropertyName("address1")>] Address1: string
          [<JsonPropertyName("city")>] City: string
          [<JsonPropertyName("stateProvince")>] StateProvince: string
          [<JsonPropertyName("postalCode")>] PostalCode: string
          [<JsonPropertyName("country")>] Country: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              LocationId = 0
              Address1 = String.Empty
              City = String.Empty
              StateProvince = String.Empty
              PostalCode = String.Empty
              Country = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `physical_addresses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `location_id` int NOT NULL,
  `address_1` varchar(200) NOT NULL,
  `city` varchar(100) NOT NULL,
  `state_province` varchar(100) NOT NULL,
  `postal_code` varchar(100) NOT NULL,
  `country` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `physical_addresses_UN` (`reference`),
  KEY `physical_addresses_FK` (`location_id`),
  CONSTRAINT `physical_addresses_FK` FOREIGN KEY (`location_id`) REFERENCES `locations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              location_id,
              address_1,
              city,
              state_province,
              postal_code,
              country
        FROM physical_addresses
        """
    
        static member TableName() = "physical_addresses"
    
    type PublicRequestsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("requestedOn")>] RequestedOn: DateTime
          [<JsonPropertyName("contactEmail")>] ContactEmail: string
          [<JsonPropertyName("contactFirstName")>] ContactFirstName: string
          [<JsonPropertyName("contactLastName")>] ContactLastName: string
          [<JsonPropertyName("organisationId")>] OrganisationId: int }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              RequestedOn = DateTime.UtcNow
              ContactEmail = String.Empty
              ContactFirstName = String.Empty
              ContactLastName = String.Empty
              OrganisationId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `public_requests` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `requested_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `contact_email` varchar(100) NOT NULL,
  `contact_first_name` varchar(100) NOT NULL,
  `contact_last_name` varchar(100) NOT NULL,
  `organisation_id` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `public_requests_FK` (`organisation_id`),
  CONSTRAINT `public_requests_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              requested_on,
              contact_email,
              contact_first_name,
              contact_last_name,
              organisation_id
        FROM public_requests
        """
    
        static member TableName() = "public_requests"
    
    type RegularSchedulesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceLocationId")>] ServiceLocationId: int
          [<JsonPropertyName("weekday")>] Weekday: int
          [<JsonPropertyName("opensAt")>] OpensAt: DateTime option
          [<JsonPropertyName("closesAt")>] ClosesAt: DateTime option
          [<JsonPropertyName("validFrom")>] ValidFrom: DateTime option
          [<JsonPropertyName("validTo")>] ValidTo: DateTime option
          [<JsonPropertyName("dtstart")>] Dtstart: DateTime option
          [<JsonPropertyName("freq")>] Freq: string option
          [<JsonPropertyName("interval")>] Interval: int option
          [<JsonPropertyName("byday")>] Byday: string option
          [<JsonPropertyName("bymonthday")>] Bymonthday: int option
          [<JsonPropertyName("description")>] Description: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceLocationId = 0
              Weekday = 0
              OpensAt = None
              ClosesAt = None
              ValidFrom = None
              ValidTo = None
              Dtstart = None
              Freq = None
              Interval = None
              Byday = None
              Bymonthday = None
              Description = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `regular_schedules` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_location_id` int NOT NULL,
  `weekday` int NOT NULL,
  `opens_at` datetime DEFAULT NULL,
  `closes_at` date DEFAULT NULL,
  `valid_from` datetime DEFAULT NULL,
  `valid_to` datetime DEFAULT NULL,
  `dtstart` datetime DEFAULT NULL,
  `freq` varchar(10) DEFAULT NULL,
  `interval` int DEFAULT NULL,
  `byday` varchar(100) DEFAULT NULL,
  `bymonthday` int DEFAULT NULL,
  `description` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `regular_schedules_UN` (`reference`),
  KEY `regular_schedules_FK` (`service_location_id`),
  CONSTRAINT `regular_schedules_FK` FOREIGN KEY (`service_location_id`) REFERENCES `service_locations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_location_id,
              weekday,
              opens_at,
              closes_at,
              valid_from,
              valid_to,
              dtstart,
              freq,
              interval,
              byday,
              bymonthday,
              description
        FROM regular_schedules
        """
    
        static member TableName() = "regular_schedules"
    
    type ResourcesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("briefDescription")>] BriefDescription: string
          [<JsonPropertyName("fullDescription")>] FullDescription: string }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              Url = String.Empty
              BriefDescription = String.Empty
              FullDescription = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `resources` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `brief_description` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `full_description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=46 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              url,
              brief_description,
              full_description
        FROM resources
        """
    
        static member TableName() = "resources"
    
    type ServiceAccessibilityRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("locationId")>] LocationId: int
          [<JsonPropertyName("accessibility")>] Accessibility: string }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              LocationId = 0
              Accessibility = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_accessibility` (
  `id` int NOT NULL,
  `reference` varchar(36) NOT NULL,
  `location_id` int NOT NULL,
  `accessibility` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_accessibility_UN` (`reference`),
  KEY `service_accessibility_FK` (`location_id`),
  CONSTRAINT `service_accessibility_FK` FOREIGN KEY (`location_id`) REFERENCES `locations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              location_id,
              accessibility
        FROM service_accessibility
        """
    
        static member TableName() = "service_accessibility"
    
    type ServiceAreasRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("serviceArea")>] ServiceArea: string
          [<JsonPropertyName("extent")>] Extent: string
          [<JsonPropertyName("uri")>] Uri: string }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              ServiceArea = String.Empty
              Extent = String.Empty
              Uri = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_areas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `service_area` varchar(100) NOT NULL,
  `extent` varchar(100) NOT NULL,
  `uri` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_areas_UN` (`reference`),
  KEY `service_areas_FK` (`service_id`),
  CONSTRAINT `service_areas_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              service_area,
              extent,
              uri
        FROM service_areas
        """
    
        static member TableName() = "service_areas"
    
    type ServiceContactsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("title")>] Title: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              Name = String.Empty
              Title = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_contacts` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `title` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_contacts_UN` (`reference`),
  KEY `service_contacts_FK` (`service_id`),
  CONSTRAINT `service_contacts_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              name,
              title
        FROM service_contacts
        """
    
        static member TableName() = "service_contacts"
    
    type ServiceLangangesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("language")>] Language: string }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              Language = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_langanges` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `language` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_langanges_UN` (`reference`),
  KEY `service_langanges_FK` (`service_id`),
  CONSTRAINT `service_langanges_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              language
        FROM service_langanges
        """
    
        static member TableName() = "service_langanges"
    
    type ServiceLocationsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("locationId")>] LocationId: int }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              LocationId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_locations` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `location_id` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_locations_UN` (`reference`),
  KEY `service_locations_FK` (`service_id`),
  KEY `service_locations_FK_1` (`location_id`),
  CONSTRAINT `service_locations_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`),
  CONSTRAINT `service_locations_FK_1` FOREIGN KEY (`location_id`) REFERENCES `locations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              location_id
        FROM service_locations
        """
    
        static member TableName() = "service_locations"
    
    type ServiceReviewsRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("reivewerOrganisationId")>] ReivewerOrganisationId: int
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("date")>] Date: DateTime
          [<JsonPropertyName("score")>] Score: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("widget")>] Widget: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              ReivewerOrganisationId = 0
              Title = String.Empty
              Description = String.Empty
              Date = DateTime.UtcNow
              Score = String.Empty
              Url = String.Empty
              Widget = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_reviews` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `reivewer_organisation_id` int NOT NULL,
  `title` varchar(100) NOT NULL,
  `description` varchar(1000) NOT NULL,
  `date` datetime NOT NULL,
  `score` varchar(100) NOT NULL,
  `url` varchar(100) NOT NULL,
  `widget` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_reviews_UN` (`reference`),
  KEY `service_reviews_FK` (`service_id`),
  KEY `service_reviews_FK_1` (`reivewer_organisation_id`),
  CONSTRAINT `service_reviews_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`),
  CONSTRAINT `service_reviews_FK_1` FOREIGN KEY (`reivewer_organisation_id`) REFERENCES `organisations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              reivewer_organisation_id,
              title,
              description,
              date,
              score,
              url,
              widget
        FROM service_reviews
        """
    
        static member TableName() = "service_reviews"
    
    type ServiceStatusRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_status` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_status_UN` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3 COMMENT='This is a seeded table of service status from Open Referral UK'
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name
        FROM service_status
        """
    
        static member TableName() = "service_status"
    
    type ServiceTaxonomiesRecord =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("taxonomyId")>] TaxonomyId: int }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              TaxonomyId = 0 }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_taxonomies` (
  `reference` varchar(36) NOT NULL,
  `service_id` int NOT NULL,
  `taxonomy_id` int NOT NULL,
  PRIMARY KEY (`service_id`,`taxonomy_id`),
  UNIQUE KEY `service_taxonomy_UN` (`reference`),
  KEY `service_taxonomy_FK_1` (`taxonomy_id`),
  CONSTRAINT `service_taxonomy_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`),
  CONSTRAINT `service_taxonomy_FK_1` FOREIGN KEY (`taxonomy_id`) REFERENCES `taxonomies` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              reference,
              service_id,
              taxonomy_id
        FROM service_taxonomies
        """
    
        static member TableName() = "service_taxonomies"
    
    type ServiceUpdatesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("newName")>] NewName: string
          [<JsonPropertyName("newDescription")>] NewDescription: string
          [<JsonPropertyName("newUrl")>] NewUrl: string option
          [<JsonPropertyName("newEmail")>] NewEmail: string option
          [<JsonPropertyName("newFees")>] NewFees: string option
          [<JsonPropertyName("newAccreditations")>] NewAccreditations: string option
          [<JsonPropertyName("newDeliverableTypeId")>] NewDeliverableTypeId: int
          [<JsonPropertyName("newAssuredDate")>] NewAssuredDate: DateTime option
          [<JsonPropertyName("newAttendingTypeId")>] NewAttendingTypeId: int
          [<JsonPropertyName("newAttendingAccessId")>] NewAttendingAccessId: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("active")>] Active: byte
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("applied")>] Applied: byte
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("appliedOn")>] AppliedOn: DateTime option
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("rejectedBy")>] RejectedBy: int option
          [<JsonPropertyName("rejectedReason")>] RejectedReason: string option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              ServiceId = 0
              NewName = String.Empty
              NewDescription = String.Empty
              NewUrl = None
              NewEmail = None
              NewFees = None
              NewAccreditations = None
              NewDeliverableTypeId = 0
              NewAssuredDate = None
              NewAttendingTypeId = 0
              NewAttendingAccessId = 0
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              Active = 0uy
              Approved = 0uy
              Applied = 0uy
              ApprovedOn = None
              ApprovedBy = None
              AppliedOn = None
              RejectedOn = None
              RejectedBy = None
              RejectedReason = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `service_updates` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) COLLATE utf8_unicode_ci NOT NULL,
  `service_id` int NOT NULL,
  `new_name` text COLLATE utf8_unicode_ci NOT NULL,
  `new_description` varchar(1000) COLLATE utf8_unicode_ci NOT NULL,
  `new_url` varchar(100) COLLATE utf8_unicode_ci DEFAULT NULL,
  `new_email` varchar(100) COLLATE utf8_unicode_ci DEFAULT NULL,
  `new_fees` varchar(100) COLLATE utf8_unicode_ci DEFAULT NULL,
  `new_accreditations` varchar(100) COLLATE utf8_unicode_ci DEFAULT NULL,
  `new_deliverable_type_id` int NOT NULL,
  `new_assured_date` datetime DEFAULT NULL,
  `new_attending_type_id` int NOT NULL,
  `new_attending_access_id` int NOT NULL,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` int NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `approved` tinyint(1) NOT NULL DEFAULT '0',
  `applied` tinyint(1) NOT NULL DEFAULT '0',
  `approved_on` datetime DEFAULT NULL,
  `approved_by` int DEFAULT NULL,
  `applied_on` datetime DEFAULT NULL,
  `rejected_on` datetime DEFAULT NULL,
  `rejected_by` int DEFAULT NULL,
  `rejected_reason` varchar(100) COLLATE utf8_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `service_updates_UN` (`reference`),
  KEY `service_updates_FK` (`service_id`),
  KEY `service_updates_FK_1` (`new_deliverable_type_id`),
  KEY `service_updates_FK_2` (`new_attending_type_id`),
  KEY `service_updates_FK_3` (`new_attending_access_id`),
  KEY `service_updates_FK_4` (`created_by`),
  KEY `service_updates_FK_5` (`approved_by`),
  KEY `service_updates_FK_6` (`rejected_by`),
  CONSTRAINT `service_updates_FK` FOREIGN KEY (`service_id`) REFERENCES `services` (`id`),
  CONSTRAINT `service_updates_FK_1` FOREIGN KEY (`new_deliverable_type_id`) REFERENCES `deliverable_types` (`id`),
  CONSTRAINT `service_updates_FK_2` FOREIGN KEY (`new_attending_type_id`) REFERENCES `attending_types` (`id`),
  CONSTRAINT `service_updates_FK_3` FOREIGN KEY (`new_attending_access_id`) REFERENCES `attending_access_types` (`id`),
  CONSTRAINT `service_updates_FK_4` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`),
  CONSTRAINT `service_updates_FK_5` FOREIGN KEY (`approved_by`) REFERENCES `users` (`id`),
  CONSTRAINT `service_updates_FK_6` FOREIGN KEY (`rejected_by`) REFERENCES `users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb3 COLLATE=utf8_unicode_ci
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              service_id,
              new_name,
              new_description,
              new_url,
              new_email,
              new_fees,
              new_accreditations,
              new_deliverable_type_id,
              new_assured_date,
              new_attending_type_id,
              new_attending_access_id,
              created_on,
              created_by,
              active,
              approved,
              applied,
              approved_on,
              approved_by,
              applied_on,
              rejected_on,
              rejected_by,
              rejected_reason
        FROM service_updates
        """
    
        static member TableName() = "service_updates"
    
    type ServicesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("url")>] Url: string option
          [<JsonPropertyName("email")>] Email: string option
          [<JsonPropertyName("statusId")>] StatusId: int
          [<JsonPropertyName("fees")>] Fees: string option
          [<JsonPropertyName("accreditations")>] Accreditations: string option
          [<JsonPropertyName("deliverableTypeId")>] DeliverableTypeId: int
          [<JsonPropertyName("assuredDate")>] AssuredDate: DateTime option
          [<JsonPropertyName("attendingTypeId")>] AttendingTypeId: int
          [<JsonPropertyName("attendingAccessId")>] AttendingAccessId: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("active")>] Active: byte
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("rejectedBy")>] RejectedBy: int option
          [<JsonPropertyName("rejectedReason")>] RejectedReason: string option
          [<JsonPropertyName("lastUpdated")>] LastUpdated: DateTime option }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              OrganisationId = 0
              Name = String.Empty
              Description = String.Empty
              Url = None
              Email = None
              StatusId = 0
              Fees = None
              Accreditations = None
              DeliverableTypeId = 0
              AssuredDate = None
              AttendingTypeId = 0
              AttendingAccessId = 0
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              Active = 0uy
              Approved = 0uy
              ApprovedOn = None
              ApprovedBy = None
              RejectedOn = None
              RejectedBy = None
              RejectedReason = None
              LastUpdated = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `services` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `organisation_id` int NOT NULL,
  `name` text NOT NULL,
  `description` varchar(1000) NOT NULL,
  `url` varchar(100) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `status_id` int NOT NULL,
  `fees` varchar(100) DEFAULT NULL,
  `accreditations` varchar(100) DEFAULT NULL,
  `deliverable_type_id` int NOT NULL,
  `assured_date` datetime DEFAULT NULL,
  `attending_type_id` int NOT NULL,
  `attending_access_id` int NOT NULL,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` int NOT NULL,
  `active` tinyint(1) NOT NULL DEFAULT '1',
  `approved` tinyint(1) NOT NULL DEFAULT '0',
  `approved_on` datetime DEFAULT NULL,
  `approved_by` int DEFAULT NULL,
  `rejected_on` datetime DEFAULT NULL,
  `rejected_by` int DEFAULT NULL,
  `rejected_reason` varchar(100) DEFAULT NULL,
  `last_updated` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `services_UN` (`reference`),
  KEY `services_FK` (`organisation_id`),
  KEY `services_FK_1` (`status_id`),
  KEY `services_FK_2` (`deliverable_type_id`),
  KEY `services_FK_3` (`attending_type_id`),
  KEY `services_FK_4` (`attending_access_id`),
  KEY `services_FK_5` (`created_by`),
  KEY `services_FK_6` (`approved_by`),
  KEY `services_FK_7` (`rejected_by`),
  CONSTRAINT `services_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `services_FK_1` FOREIGN KEY (`status_id`) REFERENCES `service_status` (`id`),
  CONSTRAINT `services_FK_2` FOREIGN KEY (`deliverable_type_id`) REFERENCES `deliverable_types` (`id`),
  CONSTRAINT `services_FK_3` FOREIGN KEY (`attending_type_id`) REFERENCES `attending_types` (`id`),
  CONSTRAINT `services_FK_4` FOREIGN KEY (`attending_access_id`) REFERENCES `attending_access_types` (`id`),
  CONSTRAINT `services_FK_5` FOREIGN KEY (`created_by`) REFERENCES `users` (`id`),
  CONSTRAINT `services_FK_6` FOREIGN KEY (`approved_by`) REFERENCES `users` (`id`),
  CONSTRAINT `services_FK_7` FOREIGN KEY (`rejected_by`) REFERENCES `users` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              organisation_id,
              name,
              description,
              url,
              email,
              status_id,
              fees,
              accreditations,
              deliverable_type_id,
              assured_date,
              attending_type_id,
              attending_access_id,
              created_on,
              created_by,
              active,
              approved,
              approved_on,
              approved_by,
              rejected_on,
              rejected_by,
              rejected_reason,
              last_updated
        FROM services
        """
    
        static member TableName() = "services"
    
    type TaxonomiesRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("parentId")>] ParentId: int option
          [<JsonPropertyName("vocabulary")>] Vocabulary: string }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              Name = String.Empty
              ParentId = None
              Vocabulary = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `taxonomies` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `name` varchar(100) NOT NULL,
  `parent_id` int DEFAULT NULL,
  `vocabulary` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `taxonomies_UN` (`reference`),
  KEY `taxonomies_FK` (`parent_id`),
  CONSTRAINT `taxonomies_FK` FOREIGN KEY (`parent_id`) REFERENCES `taxonomies` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              name,
              parent_id,
              vocabulary
        FROM taxonomies
        """
    
        static member TableName() = "taxonomies"
    
    type UserOrganisationPermissionsRecord =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("userId")>] UserId: int
          [<JsonPropertyName("canEdit")>] CanEdit: byte
          [<JsonPropertyName("orgAdmin")>] OrgAdmin: byte
          [<JsonPropertyName("canAddServices")>] CanAddServices: byte }
    
        static member Blank() =
            { OrganisationId = 0
              UserId = 0
              CanEdit = 0uy
              OrgAdmin = 0uy
              CanAddServices = 0uy }
    
        static member CreateTableSql() = """
        CREATE TABLE `user_organisation_permissions` (
  `organisation_id` int NOT NULL,
  `user_id` int NOT NULL,
  `can_edit` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Can edit organisation details (still needs system approval to go public)',
  `org_admin` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Organisation admins have all permissions',
  `can_add_services` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`organisation_id`,`user_id`),
  KEY `user_organisation_permissions_FK_1` (`user_id`),
  CONSTRAINT `user_organisation_permissions_FK` FOREIGN KEY (`organisation_id`) REFERENCES `organisations` (`id`),
  CONSTRAINT `user_organisation_permissions_FK_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              organisation_id,
              user_id,
              can_edit,
              org_admin,
              can_add_services
        FROM user_organisation_permissions
        """
    
        static member TableName() = "user_organisation_permissions"
    
    type UsersRecord =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("password")>] Password: string
          [<JsonPropertyName("salt")>] Salt: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("lastLoggedIn")>] LastLoggedIn: DateTime option
          [<JsonPropertyName("email")>] Email: string
          [<JsonPropertyName("canLogIn")>] CanLogIn: byte
          [<JsonPropertyName("isAdmin")>] IsAdmin: byte
          [<JsonPropertyName("canAddOrgs")>] CanAddOrgs: byte
          [<JsonPropertyName("active")>] Active: byte }
    
        static member Blank() =
            { Id = 0
              Reference = String.Empty
              Name = String.Empty
              Password = String.Empty
              Salt = String.Empty
              CreatedOn = DateTime.UtcNow
              LastLoggedIn = None
              Email = String.Empty
              CanLogIn = 0uy
              IsAdmin = 0uy
              CanAddOrgs = 0uy
              Active = 0uy }
    
        static member CreateTableSql() = """
        CREATE TABLE `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `salt` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_on` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `last_logged_in` datetime DEFAULT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `can_log_in` tinyint(1) NOT NULL DEFAULT '0',
  `is_admin` tinyint(1) NOT NULL DEFAULT '0',
  `can_add_orgs` tinyint(1) NOT NULL DEFAULT '0',
  `active` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              name,
              password,
              salt,
              created_on,
              last_logged_in,
              email,
              can_log_in,
              is_admin,
              can_add_orgs,
              active
        FROM users
        """
    
        static member TableName() = "users"
    
module Operations =
    type AddAttendingAccessTypesParameters =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    let insertAttendingAccessTypes (parameters: AddAttendingAccessTypesParameters) (context: MySqlContext) =
        context.Insert("attending_access_types", parameters)
    
    type AddAttendingTypesParameters =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    let insertAttendingTypes (parameters: AddAttendingTypesParameters) (context: MySqlContext) =
        context.Insert("attending_types", parameters)
    
    type AddCategoriesParameters =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("reference")>] Reference: string }
    
        static member Blank() =
            { Name = String.Empty
              Reference = String.Empty }
    
    let insertCategories (parameters: AddCategoriesParameters) (context: MySqlContext) =
        context.Insert("categories", parameters)
    
    type AddContactPhoneNumbersParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("contactId")>] ContactId: int
          [<JsonPropertyName("number")>] Number: string
          [<JsonPropertyName("language")>] Language: string option }
    
        static member Blank() =
            { Reference = String.Empty
              ContactId = 0
              Number = String.Empty
              Language = None }
    
    let insertContactPhoneNumbers (parameters: AddContactPhoneNumbersParameters) (context: MySqlContext) =
        context.Insert("contact_phone_numbers", parameters)
    
    type AddCostOptionsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("validFrom")>] ValidFrom: DateTime option
          [<JsonPropertyName("validTo")>] ValidTo: DateTime option
          [<JsonPropertyName("option")>] Option: string option
          [<JsonPropertyName("amount")>] Amount: decimal option
          [<JsonPropertyName("amountDescription")>] AmountDescription: string option }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              ValidFrom = None
              ValidTo = None
              Option = None
              Amount = None
              AmountDescription = None }
    
    let insertCostOptions (parameters: AddCostOptionsParameters) (context: MySqlContext) =
        context.Insert("cost_options", parameters)
    
    type AddDeliverableTypesParameters =
        { [<JsonPropertyName("name")>] Name: string option }
    
        static member Blank() =
            { Name = None }
    
    let insertDeliverableTypes (parameters: AddDeliverableTypesParameters) (context: MySqlContext) =
        context.Insert("deliverable_types", parameters)
    
    type AddEligibilitiesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("eligibility")>] Eligibility: string
          [<JsonPropertyName("minimumAge")>] MinimumAge: int
          [<JsonPropertyName("maximumAge")>] MaximumAge: int }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              Eligibility = String.Empty
              MinimumAge = 0
              MaximumAge = 0 }
    
    let insertEligibilities (parameters: AddEligibilitiesParameters) (context: MySqlContext) =
        context.Insert("eligibilities", parameters)
    
    type AddFundingParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("source")>] Source: string }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              Source = String.Empty }
    
    let insertFunding (parameters: AddFundingParameters) (context: MySqlContext) =
        context.Insert("funding", parameters)
    
    type AddHolidayScheduleParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceLocationId")>] ServiceLocationId: int
          [<JsonPropertyName("closed")>] Closed: byte
          [<JsonPropertyName("opensAt")>] OpensAt: DateTime option
          [<JsonPropertyName("closesAt")>] ClosesAt: DateTime option
          [<JsonPropertyName("startDate")>] StartDate: DateTime option
          [<JsonPropertyName("endDate")>] EndDate: DateTime option }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceLocationId = 0
              Closed = 0uy
              OpensAt = None
              ClosesAt = None
              StartDate = None
              EndDate = None }
    
    let insertHolidaySchedule (parameters: AddHolidayScheduleParameters) (context: MySqlContext) =
        context.Insert("holiday_schedule", parameters)
    
    type AddKeywordsParameters =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    let insertKeywords (parameters: AddKeywordsParameters) (context: MySqlContext) =
        context.Insert("keywords", parameters)
    
    type AddLinkTaxonomiesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("linkType")>] LinkType: string
          [<JsonPropertyName("linkReference")>] LinkReference: string
          [<JsonPropertyName("taxonomyId")>] TaxonomyId: int }
    
        static member Blank() =
            { Reference = String.Empty
              LinkType = String.Empty
              LinkReference = String.Empty
              TaxonomyId = 0 }
    
    let insertLinkTaxonomies (parameters: AddLinkTaxonomiesParameters) (context: MySqlContext) =
        context.Insert("link_taxonomies", parameters)
    
    type AddLocationsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("latitude")>] Latitude: decimal option
          [<JsonPropertyName("longitude")>] Longitude: decimal option }
    
        static member Blank() =
            { Reference = String.Empty
              Name = String.Empty
              Description = String.Empty
              Latitude = None
              Longitude = None }
    
    let insertLocations (parameters: AddLocationsParameters) (context: MySqlContext) =
        context.Insert("locations", parameters)
    
    type AddLogsParameters =
        { [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("success")>] Success: byte
          [<JsonPropertyName("userId")>] UserId: int option
          [<JsonPropertyName("message")>] Message: string
          [<JsonPropertyName("returnCode")>] ReturnCode: int }
    
        static member Blank() =
            { CreatedOn = DateTime.UtcNow
              Success = 0uy
              UserId = None
              Message = String.Empty
              ReturnCode = 0 }
    
    let insertLogs (parameters: AddLogsParameters) (context: MySqlContext) =
        context.Insert("logs", parameters)
    
    type AddOrganisationActionTypeParameters =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("message")>] Message: string }
    
        static member Blank() =
            { Name = String.Empty
              Message = String.Empty }
    
    let insertOrganisationActionType (parameters: AddOrganisationActionTypeParameters) (context: MySqlContext) =
        context.Insert("organisation_action_type", parameters)
    
    type AddOrganisationActionsParameters =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("userId")>] UserId: int
          [<JsonPropertyName("typeId")>] TypeId: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime }
    
        static member Blank() =
            { OrganisationId = 0
              UserId = 0
              TypeId = 0
              CreatedOn = DateTime.UtcNow }
    
    let insertOrganisationActions (parameters: AddOrganisationActionsParameters) (context: MySqlContext) =
        context.Insert("organisation_actions", parameters)
    
    type AddOrganisationCategoriesParameters =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("categoryId")>] CategoryId: int }
    
        static member Blank() =
            { OrganisationId = 0
              CategoryId = 0 }
    
    let insertOrganisationCategories (parameters: AddOrganisationCategoriesParameters) (context: MySqlContext) =
        context.Insert("organisation_categories", parameters)
    
    type AddOrganisationKeywordsParameters =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("keywordId")>] KeywordId: int }
    
        static member Blank() =
            { OrganisationId = 0
              KeywordId = 0 }
    
    let insertOrganisationKeywords (parameters: AddOrganisationKeywordsParameters) (context: MySqlContext) =
        context.Insert("organisation_keywords", parameters)
    
    type AddOrganisationResourcesParameters =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("resourceId")>] ResourceId: int }
    
        static member Blank() =
            { OrganisationId = 0
              ResourceId = 0 }
    
    let insertOrganisationResources (parameters: AddOrganisationResourcesParameters) (context: MySqlContext) =
        context.Insert("organisation_resources", parameters)
    
    type AddOrganisationUpdatesParameters =
        { [<JsonPropertyName("orgId")>] OrgId: int
          [<JsonPropertyName("newName")>] NewName: string
          [<JsonPropertyName("newDescription")>] NewDescription: string
          [<JsonPropertyName("newWebsite")>] NewWebsite: string
          [<JsonPropertyName("newPhone")>] NewPhone: string
          [<JsonPropertyName("newEmail")>] NewEmail: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("applied")>] Applied: byte
          [<JsonPropertyName("appliedOn")>] AppliedOn: DateTime option
          [<JsonPropertyName("rejectionReason")>] RejectionReason: string option
          [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("lastUpdated")>] LastUpdated: DateTime
          [<JsonPropertyName("rejectedBy")>] RejectedBy: double option
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("active")>] Active: byte }
    
        static member Blank() =
            { OrgId = 0
              NewName = String.Empty
              NewDescription = String.Empty
              NewWebsite = String.Empty
              NewPhone = String.Empty
              NewEmail = String.Empty
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              Approved = 0uy
              ApprovedOn = None
              ApprovedBy = None
              Applied = 0uy
              AppliedOn = None
              RejectionReason = None
              Reference = String.Empty
              LastUpdated = DateTime.UtcNow
              RejectedBy = None
              RejectedOn = None
              Active = 0uy }
    
    let insertOrganisationUpdates (parameters: AddOrganisationUpdatesParameters) (context: MySqlContext) =
        context.Insert("organisation_updates", parameters)
    
    type AddOrganisationsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("phone")>] Phone: string option
          [<JsonPropertyName("email")>] Email: string option
          [<JsonPropertyName("website")>] Website: string option
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("active")>] Active: byte
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("rejectedBy")>] RejectedBy: int option
          [<JsonPropertyName("rejectionReason")>] RejectionReason: string option
          [<JsonPropertyName("lastUpdated")>] LastUpdated: DateTime
          [<JsonPropertyName("fromPublic")>] FromPublic: byte }
    
        static member Blank() =
            { Reference = String.Empty
              Name = String.Empty
              Phone = None
              Email = None
              Website = None
              Description = String.Empty
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              ApprovedOn = None
              ApprovedBy = None
              Active = 0uy
              Approved = 0uy
              RejectedOn = None
              RejectedBy = None
              RejectionReason = None
              LastUpdated = DateTime.UtcNow
              FromPublic = 0uy }
    
    let insertOrganisations (parameters: AddOrganisationsParameters) (context: MySqlContext) =
        context.Insert("organisations", parameters)
    
    type AddPhysicalAddressesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("locationId")>] LocationId: int
          [<JsonPropertyName("address1")>] Address1: string
          [<JsonPropertyName("city")>] City: string
          [<JsonPropertyName("stateProvince")>] StateProvince: string
          [<JsonPropertyName("postalCode")>] PostalCode: string
          [<JsonPropertyName("country")>] Country: string option }
    
        static member Blank() =
            { Reference = String.Empty
              LocationId = 0
              Address1 = String.Empty
              City = String.Empty
              StateProvince = String.Empty
              PostalCode = String.Empty
              Country = None }
    
    let insertPhysicalAddresses (parameters: AddPhysicalAddressesParameters) (context: MySqlContext) =
        context.Insert("physical_addresses", parameters)
    
    type AddPublicRequestsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("requestedOn")>] RequestedOn: DateTime
          [<JsonPropertyName("contactEmail")>] ContactEmail: string
          [<JsonPropertyName("contactFirstName")>] ContactFirstName: string
          [<JsonPropertyName("contactLastName")>] ContactLastName: string
          [<JsonPropertyName("organisationId")>] OrganisationId: int }
    
        static member Blank() =
            { Reference = String.Empty
              RequestedOn = DateTime.UtcNow
              ContactEmail = String.Empty
              ContactFirstName = String.Empty
              ContactLastName = String.Empty
              OrganisationId = 0 }
    
    let insertPublicRequests (parameters: AddPublicRequestsParameters) (context: MySqlContext) =
        context.Insert("public_requests", parameters)
    
    type AddRegularSchedulesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceLocationId")>] ServiceLocationId: int
          [<JsonPropertyName("weekday")>] Weekday: int
          [<JsonPropertyName("opensAt")>] OpensAt: DateTime option
          [<JsonPropertyName("closesAt")>] ClosesAt: DateTime option
          [<JsonPropertyName("validFrom")>] ValidFrom: DateTime option
          [<JsonPropertyName("validTo")>] ValidTo: DateTime option
          [<JsonPropertyName("dtstart")>] Dtstart: DateTime option
          [<JsonPropertyName("freq")>] Freq: string option
          [<JsonPropertyName("interval")>] Interval: int option
          [<JsonPropertyName("byday")>] Byday: string option
          [<JsonPropertyName("bymonthday")>] Bymonthday: int option
          [<JsonPropertyName("description")>] Description: string option }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceLocationId = 0
              Weekday = 0
              OpensAt = None
              ClosesAt = None
              ValidFrom = None
              ValidTo = None
              Dtstart = None
              Freq = None
              Interval = None
              Byday = None
              Bymonthday = None
              Description = None }
    
    let insertRegularSchedules (parameters: AddRegularSchedulesParameters) (context: MySqlContext) =
        context.Insert("regular_schedules", parameters)
    
    type AddResourcesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("briefDescription")>] BriefDescription: string
          [<JsonPropertyName("fullDescription")>] FullDescription: string }
    
        static member Blank() =
            { Reference = String.Empty
              Url = String.Empty
              BriefDescription = String.Empty
              FullDescription = String.Empty }
    
    let insertResources (parameters: AddResourcesParameters) (context: MySqlContext) =
        context.Insert("resources", parameters)
    
    type AddServiceAccessibilityParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("locationId")>] LocationId: int
          [<JsonPropertyName("accessibility")>] Accessibility: string }
    
        static member Blank() =
            { Reference = String.Empty
              LocationId = 0
              Accessibility = String.Empty }
    
    let insertServiceAccessibility (parameters: AddServiceAccessibilityParameters) (context: MySqlContext) =
        context.Insert("service_accessibility", parameters)
    
    type AddServiceAreasParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("serviceArea")>] ServiceArea: string
          [<JsonPropertyName("extent")>] Extent: string
          [<JsonPropertyName("uri")>] Uri: string }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              ServiceArea = String.Empty
              Extent = String.Empty
              Uri = String.Empty }
    
    let insertServiceAreas (parameters: AddServiceAreasParameters) (context: MySqlContext) =
        context.Insert("service_areas", parameters)
    
    type AddServiceContactsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("title")>] Title: string option }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              Name = String.Empty
              Title = None }
    
    let insertServiceContacts (parameters: AddServiceContactsParameters) (context: MySqlContext) =
        context.Insert("service_contacts", parameters)
    
    type AddServiceLangangesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("language")>] Language: string }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              Language = String.Empty }
    
    let insertServiceLanganges (parameters: AddServiceLangangesParameters) (context: MySqlContext) =
        context.Insert("service_langanges", parameters)
    
    type AddServiceLocationsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("locationId")>] LocationId: int }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              LocationId = 0 }
    
    let insertServiceLocations (parameters: AddServiceLocationsParameters) (context: MySqlContext) =
        context.Insert("service_locations", parameters)
    
    type AddServiceReviewsParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("reivewerOrganisationId")>] ReivewerOrganisationId: int
          [<JsonPropertyName("title")>] Title: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("date")>] Date: DateTime
          [<JsonPropertyName("score")>] Score: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("widget")>] Widget: string option }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              ReivewerOrganisationId = 0
              Title = String.Empty
              Description = String.Empty
              Date = DateTime.UtcNow
              Score = String.Empty
              Url = String.Empty
              Widget = None }
    
    let insertServiceReviews (parameters: AddServiceReviewsParameters) (context: MySqlContext) =
        context.Insert("service_reviews", parameters)
    
    type AddServiceStatusParameters =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    let insertServiceStatus (parameters: AddServiceStatusParameters) (context: MySqlContext) =
        context.Insert("service_status", parameters)
    
    type AddServiceTaxonomiesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("taxonomyId")>] TaxonomyId: int }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              TaxonomyId = 0 }
    
    let insertServiceTaxonomies (parameters: AddServiceTaxonomiesParameters) (context: MySqlContext) =
        context.Insert("service_taxonomies", parameters)
    
    type AddServiceUpdatesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("serviceId")>] ServiceId: int
          [<JsonPropertyName("newName")>] NewName: string
          [<JsonPropertyName("newDescription")>] NewDescription: string
          [<JsonPropertyName("newUrl")>] NewUrl: string option
          [<JsonPropertyName("newEmail")>] NewEmail: string option
          [<JsonPropertyName("newFees")>] NewFees: string option
          [<JsonPropertyName("newAccreditations")>] NewAccreditations: string option
          [<JsonPropertyName("newDeliverableTypeId")>] NewDeliverableTypeId: int
          [<JsonPropertyName("newAssuredDate")>] NewAssuredDate: DateTime option
          [<JsonPropertyName("newAttendingTypeId")>] NewAttendingTypeId: int
          [<JsonPropertyName("newAttendingAccessId")>] NewAttendingAccessId: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("active")>] Active: byte
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("applied")>] Applied: byte
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("appliedOn")>] AppliedOn: DateTime option
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("rejectedBy")>] RejectedBy: int option
          [<JsonPropertyName("rejectedReason")>] RejectedReason: string option }
    
        static member Blank() =
            { Reference = String.Empty
              ServiceId = 0
              NewName = String.Empty
              NewDescription = String.Empty
              NewUrl = None
              NewEmail = None
              NewFees = None
              NewAccreditations = None
              NewDeliverableTypeId = 0
              NewAssuredDate = None
              NewAttendingTypeId = 0
              NewAttendingAccessId = 0
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              Active = 0uy
              Approved = 0uy
              Applied = 0uy
              ApprovedOn = None
              ApprovedBy = None
              AppliedOn = None
              RejectedOn = None
              RejectedBy = None
              RejectedReason = None }
    
    let insertServiceUpdates (parameters: AddServiceUpdatesParameters) (context: MySqlContext) =
        context.Insert("service_updates", parameters)
    
    type AddServicesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("description")>] Description: string
          [<JsonPropertyName("url")>] Url: string option
          [<JsonPropertyName("email")>] Email: string option
          [<JsonPropertyName("statusId")>] StatusId: int
          [<JsonPropertyName("fees")>] Fees: string option
          [<JsonPropertyName("accreditations")>] Accreditations: string option
          [<JsonPropertyName("deliverableTypeId")>] DeliverableTypeId: int
          [<JsonPropertyName("assuredDate")>] AssuredDate: DateTime option
          [<JsonPropertyName("attendingTypeId")>] AttendingTypeId: int
          [<JsonPropertyName("attendingAccessId")>] AttendingAccessId: int
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("createdBy")>] CreatedBy: int
          [<JsonPropertyName("active")>] Active: byte
          [<JsonPropertyName("approved")>] Approved: byte
          [<JsonPropertyName("approvedOn")>] ApprovedOn: DateTime option
          [<JsonPropertyName("approvedBy")>] ApprovedBy: int option
          [<JsonPropertyName("rejectedOn")>] RejectedOn: DateTime option
          [<JsonPropertyName("rejectedBy")>] RejectedBy: int option
          [<JsonPropertyName("rejectedReason")>] RejectedReason: string option
          [<JsonPropertyName("lastUpdated")>] LastUpdated: DateTime option }
    
        static member Blank() =
            { Reference = String.Empty
              OrganisationId = 0
              Name = String.Empty
              Description = String.Empty
              Url = None
              Email = None
              StatusId = 0
              Fees = None
              Accreditations = None
              DeliverableTypeId = 0
              AssuredDate = None
              AttendingTypeId = 0
              AttendingAccessId = 0
              CreatedOn = DateTime.UtcNow
              CreatedBy = 0
              Active = 0uy
              Approved = 0uy
              ApprovedOn = None
              ApprovedBy = None
              RejectedOn = None
              RejectedBy = None
              RejectedReason = None
              LastUpdated = None }
    
    let insertServices (parameters: AddServicesParameters) (context: MySqlContext) =
        context.Insert("services", parameters)
    
    type AddTaxonomiesParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("parentId")>] ParentId: int option
          [<JsonPropertyName("vocabulary")>] Vocabulary: string }
    
        static member Blank() =
            { Reference = String.Empty
              Name = String.Empty
              ParentId = None
              Vocabulary = String.Empty }
    
    let insertTaxonomies (parameters: AddTaxonomiesParameters) (context: MySqlContext) =
        context.Insert("taxonomies", parameters)
    
    type AddUserOrganisationPermissionsParameters =
        { [<JsonPropertyName("organisationId")>] OrganisationId: int
          [<JsonPropertyName("userId")>] UserId: int
          [<JsonPropertyName("canEdit")>] CanEdit: byte
          [<JsonPropertyName("orgAdmin")>] OrgAdmin: byte
          [<JsonPropertyName("canAddServices")>] CanAddServices: byte }
    
        static member Blank() =
            { OrganisationId = 0
              UserId = 0
              CanEdit = 0uy
              OrgAdmin = 0uy
              CanAddServices = 0uy }
    
    let insertUserOrganisationPermissions (parameters: AddUserOrganisationPermissionsParameters) (context: MySqlContext) =
        context.Insert("user_organisation_permissions", parameters)
    
    type AddUsersParameters =
        { [<JsonPropertyName("reference")>] Reference: string
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("password")>] Password: string
          [<JsonPropertyName("salt")>] Salt: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("lastLoggedIn")>] LastLoggedIn: DateTime option
          [<JsonPropertyName("email")>] Email: string
          [<JsonPropertyName("canLogIn")>] CanLogIn: byte
          [<JsonPropertyName("isAdmin")>] IsAdmin: byte
          [<JsonPropertyName("canAddOrgs")>] CanAddOrgs: byte
          [<JsonPropertyName("active")>] Active: byte }
    
        static member Blank() =
            { Reference = String.Empty
              Name = String.Empty
              Password = String.Empty
              Salt = String.Empty
              CreatedOn = DateTime.UtcNow
              LastLoggedIn = None
              Email = String.Empty
              CanLogIn = 0uy
              IsAdmin = 0uy
              CanAddOrgs = 0uy
              Active = 0uy }
    
    let insertUsers (parameters: AddUsersParameters) (context: MySqlContext) =
        context.Insert("users", parameters)
    
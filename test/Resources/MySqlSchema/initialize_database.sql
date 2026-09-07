CREATE DATABASE IF NOT EXISTS `test_library`;
GRANT ALL PRIVILEGES ON `test_library`.* TO 'test_user'@'%' IDENTIFIED BY 'test_password';

USE `test_library`;

CREATE TABLE `book`
(
    `book_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `title` VARCHAR(250) NOT NULL,
    `subtitle` VARCHAR(250) NULL,
    `purchase_price` DECIMAL(10, 2) NULL,
    `purchase_date` DATE NULL,
    `is_loaned` BOOLEAN NOT NULL DEFAULT FALSE,
    `notes` TEXT NULL,
    `sortable_title` VARCHAR(550) GENERATED ALWAYS AS (
        CASE
            WHEN `title` REGEXP '^(The|An|A) ' THEN
                CONCAT(
                    SUBSTR(`title`, LOCATE(' ', `title`) + 1),
                    ', ',
                    SUBSTR(`title`, 1, LOCATE(' ', `title`) - 1),
                    CASE WHEN `subtitle` IS NOT NULL AND `subtitle` <> '' THEN CONCAT(': ', `subtitle`) ELSE '' END
                )
            ELSE
                CONCAT(`title`, CASE WHEN `subtitle` IS NOT NULL AND `subtitle` <> '' THEN CONCAT(': ', `subtitle`) ELSE '' END)
        END
    ) STORED,
    PRIMARY KEY (`book_id`),
    INDEX `idx_book_sortable_title` (`sortable_title`)
);

CREATE TABLE `author`
(
    `author_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `book_id` INT UNSIGNED NOT NULL,
    `name` VARCHAR(100) NOT NULL,
    `display_order` INT UNSIGNED NOT NULL DEFAULT 0,
    PRIMARY KEY (`author_id`),
    INDEX `idx_author_book_id` (`book_id`),
    CONSTRAINT `fk_author_book_id`
        FOREIGN KEY (`book_id`)
        REFERENCES `book` (`book_id`)
);

CREATE TABLE `genre`
(
    `genre_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `parent_id` INT UNSIGNED NULL,
    `genre` VARCHAR(50) NOT NULL,
    PRIMARY KEY (`genre_id`),
    INDEX `idx_genre_parent_id` (`parent_id`),
    CONSTRAINT `uc_genre_parent` UNIQUE (`genre`, `parent_id`),
    CONSTRAINT `fk_genre_parent_id`
        FOREIGN KEY (`parent_id`)
        REFERENCES `genre` (`genre_id`)
);

CREATE TABLE `book_genre`
(
    `book_id` INT UNSIGNED NOT NULL,
    `genre_id` INT UNSIGNED NOT NULL,
    `display_order` INT UNSIGNED NOT NULL DEFAULT 0,
    PRIMARY KEY (`book_id`, `genre_id`),
    INDEX `idx_book_genre_genre_id` (`genre_id`),
    CONSTRAINT `fk_book_genre_book_id`
        FOREIGN KEY (`book_id`)
        REFERENCES `book` (`book_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_book_genre_genre_id`
        FOREIGN KEY (`genre_id`)
        REFERENCES `genre` (`genre_id`)
        ON DELETE CASCADE
);

CREATE TABLE `book_optional_genre`
(
    `book_id` INT UNSIGNED NOT NULL,
    `genre_id` INT UNSIGNED NULL,
    `display_order` INT UNSIGNED NOT NULL DEFAULT 0,
    PRIMARY KEY (`book_id`, `genre_id`),
    INDEX `idx_book_optional_genre_genre_id` (`genre_id`),
    CONSTRAINT `fk_book_optional_genre_book_id`
        FOREIGN KEY (`book_id`)
        REFERENCES `book` (`book_id`)
        ON DELETE CASCADE,
    CONSTRAINT `fk_book_optional_genre_genre_id`
        FOREIGN KEY (`genre_id`)
        REFERENCES `genre` (`genre_id`)
        ON DELETE CASCADE
);

CREATE
    SQL SECURITY DEFINER
VIEW `full_genre`
AS
    WITH RECURSIVE cte (`genre_id`, `genre`, `depth`)
    AS
    (
        SELECT
            `genre_id` AS `genre_id`,
            CAST(`genre` AS CHAR(1000)) AS `genre`,
            0 AS `depth`
        FROM `genre`
        WHERE `parent_id` IS NULL
        UNION ALL
        SELECT
            `g`.`genre_id` AS `genre_id`,
            CONCAT(`cte`.`genre`, ' › ', `g`.`genre`) AS `genre`,
            `cte`.`depth` + 1 AS `depth`
        FROM `genre` AS `g`
        INNER JOIN `cte`
            ON `g`.`parent_id` = `cte`.`genre_id`
        WHERE `cte`.`depth` < 10
    )
    SELECT
        `cte`.`genre_id`,
        `cte`.`genre`
    FROM `cte`;

-- ============================================================
-- Seed data
-- ============================================================

INSERT INTO `genre` (`genre_id`, `parent_id`, `genre`) VALUES
-- Top-level
(1,  NULL, 'Fiction'),
(2,  NULL, 'Non-Fiction'),
(3,  NULL, 'Science & Technology'),
-- Fiction sub-genres
(4,  1,    'Science Fiction'),
(5,  1,    'Fantasy'),
(6,  1,    'Historical Fiction'),
(7,  1,    'Mystery & Thriller'),
(8,  1,    'Literary Fiction'),
-- Science Fiction sub-genres
(9,  4,    'Space Opera'),
(10, 4,    'Cyberpunk'),
(11, 4,    'Hard Science Fiction'),
-- Fantasy sub-genres
(12, 5,    'Epic Fantasy'),
(13, 5,    'Urban Fantasy'),
-- Non-Fiction sub-genres
(14, 2,    'Biography & Memoir'),
(15, 2,    'History'),
(16, 2,    'Philosophy'),
(17, 2,    'Science'),
-- Science & Technology sub-genres
(18, 3,    'Computer Science'),
(19, 3,    'Mathematics'),
(20, 3,    'Physics');

INSERT INTO `book` (`book_id`, `title`, `subtitle`, `purchase_price`, `purchase_date`, `is_loaned`, `notes`) VALUES
(1,  'Dune',                              NULL,                                          12.99,  '2023-01-15', FALSE, NULL),
(2,  'Foundation',                        NULL,                                          10.99,  '2023-01-15', FALSE, NULL),
(3,  'Neuromancer',                       NULL,                                          9.99,   '2023-02-10', FALSE, NULL),
(4,  'The Left Hand of Darkness',        NULL,                                          11.99,  '2023-02-10', TRUE,  'Loaned to Sarah'),
(5,  'Hyperion',                          NULL,                                          13.99,  '2023-03-05', FALSE, NULL),
(6,  'A Fire Upon the Deep',             NULL,                                          12.49,  '2023-03-05', FALSE, NULL),
(7,  'The Name of the Wind',             NULL,                                          14.99,  '2023-04-20', FALSE, NULL),
(8,  'The Way of Kings',                 NULL,                                          17.99,  '2023-04-20', FALSE, NULL),
(9,  'American Gods',                    NULL,                                          11.49,  '2023-05-01', TRUE,  NULL),
(10, 'The Shadow of the Wind',           NULL,                                          12.99,  '2023-05-01', FALSE, NULL),
(11, 'Shogun',                           NULL,                                          14.49,  '2023-06-15', FALSE, NULL),
(12, 'The Count of Monte Cristo',        NULL,                                          9.99,   '2023-06-15', FALSE, NULL),
(13, 'Crime and Punishment',             NULL,                                          8.99,   '2023-07-10', FALSE, NULL),
(14, 'The Girl with the Dragon Tattoo', NULL,                                          11.99,  '2023-07-10', FALSE, NULL),
(15, 'Gone Girl',                        NULL,                                          10.49,  '2023-08-01', FALSE, NULL),
(16, 'Snow Crash',                       NULL,                                          12.99,  '2023-08-01', FALSE, NULL),
(17, 'The Pragmatic Programmer',         'Your Journey to Mastery',                     34.99,  '2023-09-12', FALSE, NULL),
(18, 'Clean Code',                       'A Handbook of Agile Software Craftsmanship',  35.99,  '2023-09-12', FALSE, NULL),
(19, 'Designing Data-Intensive Applications', NULL,                                     49.99,  '2023-09-12', FALSE, NULL),
(20, 'The Hitchhiker''s Guide to the Galaxy', NULL,                                     10.99,  '2023-10-05', FALSE, NULL),
(21, 'Good Omens',                       'The Nice and Accurate Prophecies of Agnes Nutter, Witch', 13.99, '2023-10-05', FALSE, NULL),
(22, 'A Brief History of Time',          NULL,                                          12.99,  '2023-11-01', FALSE, NULL),
(23, 'Sapiens',                          'A Brief History of Humankind',                15.99,  '2023-11-01', FALSE, NULL),
(24, 'The Selfish Gene',                 NULL,                                          11.99,  '2023-11-01', FALSE, NULL),
(25, 'Thinking, Fast and Slow',          NULL,                                          14.99,  '2023-12-10', FALSE, NULL),
(26, 'Meditations',                      NULL,                                          7.99,   '2023-12-10', FALSE, NULL),
(27, 'The Art of War',                   NULL,                                          6.99,   '2024-01-08', FALSE, NULL),
(28, 'An Introduction to Algorithms',   NULL,                                          79.99,  '2024-01-08', FALSE, NULL),
(29, 'The Lean Startup',                NULL,                                          14.99,  '2024-02-14', FALSE, NULL),
(30, 'Flowers for Algernon',            NULL,                                          9.99,   '2024-02-14', FALSE, NULL);

INSERT INTO `author` (`author_id`, `book_id`, `name`, `display_order`) VALUES
-- Single-author books
(1,  1,  'Frank Herbert',            0),
(2,  2,  'Isaac Asimov',             0),
(3,  3,  'William Gibson',           0),
(4,  4,  'Ursula K. Le Guin',        0),
(5,  5,  'Dan Simmons',              0),
(6,  6,  'Vernor Vinge',             0),
(7,  7,  'Patrick Rothfuss',         0),
(8,  8,  'Brandon Sanderson',        0),
(9,  9,  'Neil Gaiman',              0),
(10, 10, 'Carlos Ruiz Zafón',        0),
(11, 11, 'James Clavell',            0),
(12, 12, 'Alexandre Dumas',          0),
(13, 13, 'Fyodor Dostoevsky',        0),
(14, 14, 'Stieg Larsson',            0),
(15, 15, 'Gillian Flynn',            0),
(16, 16, 'Neal Stephenson',          0),
(19, 19, 'Martin Kleppmann',         0),
(22, 22, 'Stephen Hawking',          0),
(23, 23, 'Yuval Noah Harari',        0),
(24, 24, 'Richard Dawkins',          0),
(25, 25, 'Daniel Kahneman',          0),
(26, 26, 'Marcus Aurelius',          0),
(27, 27, 'Sun Tzu',                  0),
(30, 30, 'Daniel Keyes',             0),
-- Multi-author books
(17, 17, 'David Thomas',             0),
(18, 17, 'Andrew Hunt',              1),
(20, 18, 'Robert C. Martin',         0),
(21, 20, 'Douglas Adams',            0),
(29, 21, 'Terry Pratchett',          0),
(31, 21, 'Neil Gaiman',              1),
(32, 28, 'Thomas H. Cormen',         0),
(33, 28, 'Charles E. Leiserson',     1),
(34, 28, 'Ronald L. Rivest',         2),
(35, 28, 'Clifford Stein',           3),
(36, 29, 'Eric Ries',                0);

INSERT INTO `book_genre` (`book_id`, `genre_id`, `display_order`) VALUES
(1,  4,  0), -- Dune: Science Fiction
(1,  9,  1), -- Dune: Space Opera
(2,  4,  0), -- Foundation: Science Fiction
(3,  4,  0), -- Neuromancer: Science Fiction
(3,  10, 1), -- Neuromancer: Cyberpunk
(4,  4,  0), -- The Left Hand of Darkness: Science Fiction
(4,  11, 1), -- The Left Hand of Darkness: Hard Science Fiction
(5,  4,  0), -- Hyperion: Science Fiction
(5,  9,  1), -- Hyperion: Space Opera
(6,  4,  0), -- A Fire Upon the Deep: Science Fiction
(6,  9,  1), -- A Fire Upon the Deep: Space Opera
(7,  5,  0), -- The Name of the Wind: Fantasy
(7,  12, 1), -- The Name of the Wind: Epic Fantasy
(8,  5,  0), -- The Way of Kings: Fantasy
(8,  12, 1), -- The Way of Kings: Epic Fantasy
(9,  5,  0), -- American Gods: Fantasy
(9,  13, 1), -- American Gods: Urban Fantasy
(10, 1,  0), -- The Shadow of the Wind: Fiction
(10, 7,  1), -- The Shadow of the Wind: Mystery & Thriller
(11, 6,  0), -- Shogun: Historical Fiction
(12, 6,  0), -- The Count of Monte Cristo: Historical Fiction
(13, 8,  0), -- Crime and Punishment: Literary Fiction
(14, 7,  0), -- The Girl with the Dragon Tattoo: Mystery & Thriller
(15, 7,  0), -- Gone Girl: Mystery & Thriller
(16, 4,  0), -- Snow Crash: Science Fiction
(16, 10, 1), -- Snow Crash: Cyberpunk
(17, 18, 0), -- The Pragmatic Programmer: Computer Science
(18, 18, 0), -- Clean Code: Computer Science
(19, 18, 0), -- Designing Data-Intensive Applications: Computer Science
(20, 4,  0), -- The Hitchhiker's Guide: Science Fiction
(21, 5,  0), -- Good Omens: Fantasy
(21, 13, 1), -- Good Omens: Urban Fantasy
(22, 17, 0), -- A Brief History of Time: Science
(22, 20, 1), -- A Brief History of Time: Physics
(23, 15, 0), -- Sapiens: History
(24, 17, 0), -- The Selfish Gene: Science
(25, 2,  0), -- Thinking, Fast and Slow: Non-Fiction
(26, 16, 0), -- Meditations: Philosophy
(27, 16, 0), -- The Art of War: Philosophy
(28, 18, 0), -- Introduction to Algorithms: Computer Science
(28, 19, 1), -- Introduction to Algorithms: Mathematics
(29, 2,  0), -- The Lean Startup: Non-Fiction
(30, 4,  0); -- Flowers for Algernon: Science Fiction

INSERT INTO `book_optional_genre` (`book_id`, `genre_id`, `display_order`) VALUES
(1,  4,  0), -- Dune: Science Fiction
(1,  9,  1), -- Dune: Space Opera
(2,  4,  0), -- Foundation: Science Fiction
(3,  4,  0), -- Neuromancer: Science Fiction
(3,  10, 1), -- Neuromancer: Cyberpunk
(4,  4,  0), -- The Left Hand of Darkness: Science Fiction
(4,  11, 1), -- The Left Hand of Darkness: Hard Science Fiction
(5,  4,  0), -- Hyperion: Science Fiction
(5,  9,  1), -- Hyperion: Space Opera
(6,  4,  0), -- A Fire Upon the Deep: Science Fiction
(6,  9,  1), -- A Fire Upon the Deep: Space Opera
(7,  5,  0), -- The Name of the Wind: Fantasy
(7,  12, 1), -- The Name of the Wind: Epic Fantasy
(8,  5,  0), -- The Way of Kings: Fantasy
(8,  12, 1), -- The Way of Kings: Epic Fantasy
(9,  5,  0), -- American Gods: Fantasy
(9,  13, 1), -- American Gods: Urban Fantasy
(10, 1,  0), -- The Shadow of the Wind: Fiction
(10, 7,  1), -- The Shadow of the Wind: Mystery & Thriller
(11, 6,  0), -- Shogun: Historical Fiction
(12, 6,  0), -- The Count of Monte Cristo: Historical Fiction
(13, 8,  0), -- Crime and Punishment: Literary Fiction
(14, 7,  0), -- The Girl with the Dragon Tattoo: Mystery & Thriller
(15, 7,  0), -- Gone Girl: Mystery & Thriller
(16, 4,  0), -- Snow Crash: Science Fiction
(16, 10, 1), -- Snow Crash: Cyberpunk
(17, 18, 0), -- The Pragmatic Programmer: Computer Science
(18, 18, 0), -- Clean Code: Computer Science
(19, 18, 0), -- Designing Data-Intensive Applications: Computer Science
(20, 4,  0), -- The Hitchhiker's Guide: Science Fiction
(21, 5,  0), -- Good Omens: Fantasy
(21, 13, 1), -- Good Omens: Urban Fantasy
(22, 17, 0), -- A Brief History of Time: Science
(22, 20, 1), -- A Brief History of Time: Physics
(23, 15, 0), -- Sapiens: History
(24, 17, 0), -- The Selfish Gene: Science
(25, 2,  0), -- Thinking, Fast and Slow: Non-Fiction
(26, 16, 0), -- Meditations: Philosophy
(27, 16, 0), -- The Art of War: Philosophy
(28, 18, 0), -- Introduction to Algorithms: Computer Science
(28, 19, 1), -- Introduction to Algorithms: Mathematics
(29, 2,  0), -- The Lean Startup: Non-Fiction
(30, 4,  0); -- Flowers for Algernon: Science Fiction


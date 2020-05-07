--1. how many female actors are there?
SELECT COUNT(*) FROM Actors WHERE sex='F' -- 32,896

--2. the number of female actors and the number of male actors as a single query
SELECT sex, COUNT(sex)
FROM Actors
GROUP BY sex

--3. movie titles and number of directors involved for movies with more than 6 directors
SELECT m.title, COUNT(m.title) AS NumberOfDirectors -- 6
FROM Movies m
JOIN MoviesToDirectors md ON m.movieid = md.movieid
GROUP BY m.title
HAVING COUNT(m.title) > 6 
ORDER BY 2 desc

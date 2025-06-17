-- Check procedure definition
SELECT text 
FROM user_source 
WHERE name = 'UPDATE_POSITION' 
ORDER BY line;

-- Check procedure parameters with more details
SELECT 
    a.argument_name,
    a.data_type,
    a.in_out,
    a.data_length,
    a.data_precision,
    a.data_scale,
    a.position
FROM 
    all_arguments a
WHERE 
    a.object_name = 'UPDATE_POSITION'
ORDER BY 
    a.position;

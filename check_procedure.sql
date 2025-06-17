-- Check if the procedure exists
SELECT object_name, object_type, status 
FROM user_objects 
WHERE object_name = 'UPDATE_POSITION';

-- Check procedure parameters
SELECT argument_name, data_type, in_out, position
FROM all_arguments
WHERE object_name = 'UPDATE_POSITION'
ORDER BY position;

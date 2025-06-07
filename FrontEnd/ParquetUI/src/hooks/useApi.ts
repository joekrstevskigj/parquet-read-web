import { useState } from 'react';

function useApi<T>(endpoint: string) {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const fetchData = async (method: 'GET' | 'POST' = 'GET', body?: string) => {

        setLoading(true);
        setError(null);

        const completeEndpoint = method === 'GET' && body
            ? `${endpoint}?${body}`
            : endpoint;


        try {
            const response = await fetch(`${import.meta.env.VITE_API_URL}${completeEndpoint}`, {
                method,
                headers: {
                    'Content-Type': 'application/json',
                },
                body: method === 'POST' ? JSON.stringify(body) : undefined,
            });

            if (!response.ok) {
                throw new Error(`Error: ${response.statusText}`);
            }

            const result = await response.json();
            setData(result);
        } catch (err: unknown) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError(String(err));
            }
        } finally {
            setLoading(false);
        }
    };

    return { data, loading, error, fetchData };
}

export default useApi;
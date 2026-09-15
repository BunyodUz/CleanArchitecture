import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';

export function LoginPage() {
  const { login } = useAuth();
  const location = useLocation();

  useEffect(() => {
    login(location.state?.returnUrl || '/');
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return <p>Redirecting to sign in&hellip;</p>;
}

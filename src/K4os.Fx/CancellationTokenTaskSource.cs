namespace K4os.Fx;

public sealed class CancellationTokenTaskSource: IDisposable
{
	private readonly TaskCompletionSource<bool> _tcs;
	private readonly CancellationTokenRegistration _registration;

	public CancellationTokenTaskSource(CancellationToken token)
	{
		_tcs = new TaskCompletionSource<bool>();
		_registration = token.Register(() => _tcs.TrySetResult(true), false);
	}

	public Task Task => _tcs.Task;

	public void Dispose()
	{
		_tcs.TrySetCanceled();
		// ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
		_registration.Dispose();
	}
}
